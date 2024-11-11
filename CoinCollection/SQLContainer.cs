using CoinCollection.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CoinCollection
{
    public enum SQLError
    {
        None,
        SQL,
        Other,
    }

    internal class SQLContainer
    {
        public readonly float ServerVersion = 0.01f;

        private readonly string _serverVersionDescription = "";

        private LocalDBMSSQLLocalDBContext _localDBContext;

        private readonly int _errorTrys = 5;
        private int _currentErrorTrys = 0;

        private SqlConnection _sqlConnection;

        private readonly string[] _tableCreationCommands = { "CDate (Id INT IDENTITY(1,1) PRIMARY KEY, Date DATE NOT NULL)",
            "Coin (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50) NOT NULL, [Original Value] MONEY NOT NULL, [Retail Value] MONEY NOT NULL, [Amount Made] INT NOT NULL, ImagePath NVARCHAR(MAX) NOT NULL DEFAULT 'No_Coin_Image.jpg', Description NVARCHAR(MAX) NOT NULL)",
            "CoinDate (Id INT IDENTITY(1,1) PRIMARY KEY, DateId INT NOT NULL, CoinId INT NOT NULL, CONSTRAINT DateFK FOREIGN KEY (DateId) REFERENCES CDate(Id), CONSTRAINT CoinFK FOREIGN KEY (CoinId) REFERENCES Coin(Id))",
            "ServerInfo (VersionId INT IDENTITY(1,1) PRIMARY KEY, VersionNumb FLOAT NOT NULL, Description NVARCHAR(MAX), LastUpdated FLOAT NOT NULL)"};

        public SQLContainer()
        {
            _sqlConnection = new SqlConnection(App.GetInstance().ConnectionString);

            var optionsBuilder = new DbContextOptionsBuilder<LocalDBMSSQLLocalDBContext>();

            optionsBuilder.UseSqlServer(App.GetInstance().ConnectionString);

            _localDBContext = new(optionsBuilder.Options);
        }

        public bool ExistingServer(string loc)
        {
            SqlConnection connect = new($@"Server=(localdb)\MSSQLLocalDB;AttachDbFilename={loc};Integrated Security=true;");

            if(ReturnCheckTable(connect))
            {
                return UpdateJsonSettingsFile(loc);
            }

            return false;
        }

        public bool NewServer(string loc)
        {
            if (File.Exists(Path.Combine(loc, "Coins.mdf")))
            {
                return ExistingServer(Path.Combine(loc, "Coins.mdf"));
            }

            SqlConnection connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

            SqlCommand command = new($"CREATE DATABASE Coins ON PRIMARY (NAME = Coins, FILENAME = '{Path.Combine(loc, "Coins.mdf")}') LOG ON (NAME = Coins_log, FILENAME = '{Path.Combine(loc, "Coins_log.ldf")}');", connect);

            try
            {
                using (connect)
                {
                    connect.Open();
                    command.ExecuteNonQuery();

                    foreach(string tableCommand in _tableCreationCommands)
                    {
                        command.CommandText = $"USE Coins; CREATE TABLE {tableCommand};";
                        command.ExecuteNonQuery();
                    }

                    command.CommandText = $"USE Coins; INSERT INTO ServerInfo (VersionNumb, Description, LastUpdated) VALUES ({ServerVersion}, '{_serverVersionDescription}', {10.1f})";
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                _currentErrorTrys++;

                if(_currentErrorTrys == _errorTrys)
                {
                    _currentErrorTrys = 0;
                    MessageBox.Show($"Too many attempts to fix error ({ex.Message})!!!");
                    return false;
                }

                if(ex.Message.Contains("already exists"))
                {
                    connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

                    using (connect)
                    {
                        connect.Open();

                        SqlCommand clearSQLServer = new ("IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Coins') DROP DATABASE [Coins];", connect);
                        
                        try
                        {
                            clearSQLServer.ExecuteNonQuery();
                        }
                        catch (SqlException clearServerErrorCode)
                        {
                            if(!clearServerErrorCode.Message.Contains("Unable to open the physical file"))
                            {
                                MessageBox.Show(clearServerErrorCode.Message);
                                return false;
                            }
                        }
                    }

                    return NewServer(loc);
                }
                else
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }

            return UpdateJsonSettingsFile(Path.Combine(loc, "Coins.mdf"));
        }

        public bool HasConnected()
        {
            return HasConnected(out _);
        }

        public bool HasConnected(out SQLError error)
        {
            try
            {
                _sqlConnection.Open();

                error = SQLError.None;

                return true;
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL error: {ex.Message}");
                MessageBox.Show(ex.Message, "SQL Error");

                error = SQLError.SQL;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error");

                error = SQLError.Other;
            }
            finally
            {
                _sqlConnection.Close();
            }

            return false;
        }

        //public void CheckServerExistance()
        public bool CheckServerExistance()
        {
            string sqlDir = App.GetInstance().SQLDir!;

            if (string.IsNullOrEmpty(sqlDir) || !File.Exists(sqlDir))
            {
                ServerSelectorWindow ssw = App.GetInstance().GetService<ServerSelectorWindow>();

                ssw.ShowDialog(WindowStartupLocation.CenterScreen, true);

                if(ssw.IsCancled)
                {
                    return false;
                }
            }

            while (true)
            {
                if (!HasConnected(out SQLError error))
                {
                    ServerSelectorWindow ssw = App.GetInstance().GetService<ServerSelectorWindow>();

                    ssw.ShowDialog(WindowStartupLocation.CenterScreen, true);

                    if (ssw.IsCancled)
                    {
                        return false;
                    }

                    /*if (error != SQLError.Other)
                    {
                        //TODO: Sort out thi piece of code
                    }
                    else
                    {
                        //TODO: Throw error message and close application!!!

                        //App.GetInstance().GetService<ServerSelectorWindow>().Show(WindowStartupLocation.CenterScreen, true);

                        ServerSelectorWindow ssw = App.GetInstance().GetService<ServerSelectorWindow>();

                        ssw.ShowDialog(WindowStartupLocation.CenterScreen, true);

                        if (ssw.IsCancled)
                        {
                            return;
                        }
                    }*/
                }
                else
                {
                    if(ReturnCheckTable(_sqlConnection))
                    {
                        break;
                    }
                }
            }

            return true;
        }

        public DataTable GetServerInfo()
        {
            _sqlConnection.Open();

            SqlDataAdapter sqlDataAdapter = new ("SELECT * FROM Coin", _sqlConnection);

            DataTable dt = new ();

            sqlDataAdapter.Fill(dt);

            _sqlConnection.Close();

            return dt;
        }

        public T GetServerColumnInfo<T>(string tableName, string columnName)
        {
            if(string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("TableName and ColumnName can not be empty!!!");
            }

            _sqlConnection.Open();

            SqlCommand cmd = new ($"USE Coins; SELECT {columnName} FROM {tableName}", _sqlConnection);
            object result = cmd.ExecuteScalar();

            _sqlConnection.Close();

            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException($"No data found in {tableName}.{columnName}");
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        private static bool ReturnCheckTable(SqlConnection connect)
        {
            return CheckTable(connect, "CDate", "Id", "Date") && CheckTable(connect, "Coin", "Id", "Name", "Original Value", "Retail Value", "Amount Made", "ImagePath", "Description") 
                && CheckTable(connect, "CoinDate", "Id", "DateId", "CoinId") && CheckTable(connect, "ServerInfo", "VersionId", "VersionNumb", "Description", "LastUpdated");
        }

        private static bool CheckTable(SqlConnection connect, string tableName, params string[] colomNames)
        {
            if(string.IsNullOrEmpty(tableName))
            {
                return false;
            }

            SqlCommand command = new($"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME ='{tableName}') SELECT 1 ELSE SELECT 0", connect);

            connect.Open();
            int result = (int)command.ExecuteScalar();

            if(result == 1)
            {
                if(colomNames.Length != 0)
                {
                    foreach(string colomName in colomNames)
                    {
                        command.CommandText = $"IF COL_LENGTH('{tableName}','{colomName}') IS NOT NULL SELECT 1 ELSE SELECT 0";

                        if((int)command.ExecuteScalar() == 0)
                        {
                            MessageBox.Show($"{connect.Database} does not have {colomName} colom in {tableName}!!!", "Error");
                            connect.Close();
                            return false;
                        }
                    }
                }

                connect.Close();
                return true;
            }

            MessageBox.Show($"{connect.Database} does not have {tableName}!!!", "Error");
            connect.Close();
            return false;
        }

        private bool UpdateJsonSettingsFile(string loc)
        {
            if(string.IsNullOrEmpty(loc))
            {
                return false;
            }

            var json = JsonNode.Parse(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));

            if (json != null)
            {
                json["SQL Dir"] = loc;

                var jsonConnectionString = json["ConnectionStrings"];

                if (jsonConnectionString != null)
                {
                    jsonConnectionString["DefaultConnection"] = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={loc};Integrated Security=True;Connect Timeout=30";
                }
                else
                {
                    json["ConnectionStrings"] = new JsonObject()
                    {
                        ["DefaultConnection"] = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={loc};Integrated Security=True;Connect Timeout=30"
                    };
                }
            }
            else
            {
                MessageBox.Show("Unable to open appsettings!!!");

                App.GetInstance().GetService<ServerSelectorWindow>().CloseWindow();

                return false;
            }

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            //App.GetInstance().UpdateAppSettings();
            App.GetInstance().ConfigWait.WaitOne();

            _sqlConnection.ConnectionString = App.GetInstance().ConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<LocalDBMSSQLLocalDBContext>();

            optionsBuilder.UseSqlServer(App.GetInstance().ConnectionString);

            _localDBContext = new(optionsBuilder.Options);

            return true;
        }
    }
}
