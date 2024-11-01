using CoinCollection.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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
        private readonly IConfiguration _config;

        private readonly string _connectionString;

        private readonly LocalDBMSSQLLocalDBContext _localDBContext;

        private readonly IHost _host;

        private readonly int _errorTrys = 5;
        private int _currentErrorTrys = 0;

        private SqlConnection _sqlConnection;

        private string _sqlDir;

        public SQLContainer(LocalDBMSSQLLocalDBContext localDBContext, IConfiguration config, IHost host)
        {
            _config = config;

            ArgumentNullException.ThrowIfNull(_config);

            _connectionString = _config.GetConnectionString("DefaultConnection") ?? string.Empty;

            _sqlDir = _config.GetValue<string>("SQL Dir") ?? string.Empty;

            _sqlConnection = new SqlConnection(_connectionString);

            _localDBContext = localDBContext;

            _host = host;

            CheckServerExistance();
        }

        public bool ExistingServer(string loc)
        {
            SqlConnection connect = new($@"Server=(localdb)\MSSQLLocalDB;AttachDbFilename={loc};Integrated Security=true;");

            if(CheckTable(connect, "CDate", "Id", "Date") && CheckTable(connect, "Coin", "Id", "Name", "Original Value", "Retail Value", "Amount Made", "Image Path", "Desciption") && CheckTable(connect, "CoinDate", "Id", "DateId", "CoinId"))
            {
                return true;
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

            var json = JsonNode.Parse(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));

            if(json != null)
            {
                loc = Path.Combine(loc, "Coins.mdf");
                json["SQL Dir"] = loc;
                //json["DefaultConnection"] = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={loc};Integrated Security=True;Connect Timeout=30";

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

                _host.Services.GetRequiredService<ServerSelectorWindow>().CloseWindow();

                return false;
            }

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            return true;
        }

        public bool HasConnected()
        {
            return HasConnected(out _);
        }

        public bool HasConnected(out SQLError error)
        {
            try
            {
                using (_sqlConnection)
                {
                    _sqlConnection.Open();

                    error = SQLError.None;

                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL error: {ex.Message}");

                error = SQLError.SQL;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");

                error = SQLError.Other;

                return false;
            }
        }

        private void CheckServerExistance()
        {
            //if(string.IsNullOrEmpty(_sqlDir) || !Directory.Exists(_sqlDir))
            if(string.IsNullOrEmpty(_sqlDir) || !File.Exists(_sqlDir))
            {
                _host.Services.GetRequiredService<ServerSelectorWindow>().Show(System.Windows.WindowStartupLocation.CenterScreen, true);

                return;
            }

            if (!HasConnected(out SQLError error))
            {
                if(error != SQLError.Other)
                {
                    /*string temp = Path.Combine(Directory.GetCurrentDirectory(), "Database");

                    if(!Directory.Exists(temp))
                    {
                        Directory.CreateDirectory(temp);
                    }

                    if(!Directory.Exists($"{Directory.GetCurrentDirectory()}\\Coins.mdf"))
                    {
                        
                    }

                    SqlConnection connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

                    SqlCommand command = new($@"""CREATE DATABASE Coins ON PRIMARY (NAME = Coins_data, FILENAME = {Path.Combine(temp, "Coins.mdf")}) LOG ON (NAME = Coins_log, FILENAME = {Path.Combine(temp, "Coins.ldf")}); """, connect);

                    using (connect)
                    {
                        connect.Open();
                        command.ExecuteNonQuery();
                    }*/
                }
                else
                {
                    //TODO: Throw error message and close application!!!
                }
            }
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
                        command.CommandText = $"IF COL_LENGTH('{tableName}','{colomName}') SELECT 1 ELSE SELECT 0";

                        if((int)command.ExecuteScalar() == 0)
                        {
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
    }
}
