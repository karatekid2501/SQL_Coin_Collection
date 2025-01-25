using CoinCollection.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO;
using System.Windows;

namespace CoinCollection
{
    public enum SQLError
    {
        None,
        SQL,
        Other,
    }

    public class SQLContainer
    {
        public readonly float ServerVersion = 0.01f;

        private readonly string _serverVersionDescription = "";

        private LocalDBMSSQLLocalDBContext _localDBContext;

        private readonly int _errorTrys = 5;
        private int _currentErrorTrys = 0;

        private SqlConnection _sqlConnection;

        private readonly string[] _tableCreationCommands = { "CDate (Id INT IDENTITY(1,1) PRIMARY KEY, Date DATE NOT NULL)",
            //"Coin (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50) NOT NULL, [Original Value] MONEY NOT NULL, [Retail Value] MONEY NOT NULL, [Amount Made] INT NOT NULL, ImagePath NVARCHAR(MAX) NOT NULL DEFAULT 'No_Coin_Image.jpg', Description NVARCHAR(MAX) NOT NULL)",
            "Coin (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50) NOT NULL, Description NVARCHAR(MAX) NOT NULL, [Amount Made] INT NOT NULL, [Currency Type] NVARCHAR(MAX) NOT NULL, [Original Value] NVARCHAR(MAX) NOT NULL, [Retail Value] NVARCHAR(MAX) NOT NULL, ImagePath NVARCHAR(MAX) NOT NULL DEFAULT 'No_Coin_Image.jpg')",
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
                    //MessageBox.Show($"Too many attempts to fix error ({ex.Message})!!!");

                    App.GetInstance().Report.ShowMessage($"Too many attempts to fix error ({ex.Message})", "Warning", ReportSeverity.Warning);
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
                                //MessageBox.Show(clearServerErrorCode.Message);

                                App.GetInstance().Report.ShowMessage(clearServerErrorCode, "Warning", ReportSeverity.Warning);

                                return false;
                            }
                        }
                    }

                    return NewServer(loc);
                }
                else
                {
                    //MessageBox.Show(ex.Message);

                    App.GetInstance().Report.ShowMessage(ex, "Warning", ReportSeverity.Warning);

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
                //Debug.WriteLine($"SQL error: {ex.Message}");
                //MessageBox.Show(ex.Message, "SQL Error");

                App.GetInstance().Report.ShowMessage(ex, "Warning", ReportSeverity.Warning);

                error = SQLError.SQL;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"General error: {ex.Message}");
                //MessageBox.Show(ex.Message, "Error");

                App.GetInstance().Report.ShowMessage(ex, "Warning", ReportSeverity.Warning);

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

        public bool CheckMoneyNameExists(string currencyName)
        {
            _sqlConnection.Open();

            bool exists = false;

            using (SqlCommand command = new($"SELECT COUNT(*) FROM Coin WHERE Name = '{currencyName}'", _sqlConnection))
            {
                exists = (int)command.ExecuteScalar() > 0;
            }

            _sqlConnection.Close();

            return exists;
        }

        public bool SubmitNew(ServerDataContainer newData, out Exception e)
        {
            try
            {
                _sqlConnection.Open();

                SqlCommand command = new($"INSERT INTO Coin VALUES {newData.CoinInfo}", _sqlConnection);

                command.ExecuteNonQuery();

                _sqlConnection.Close();

                e = new();

                return true;
            }
            catch (Exception foundExeption)
            {
                _sqlConnection.Close();

                e = foundExeption;

                return false;
            }
        }

        public bool SubmitAltered(ServerDataContainer modifiedData)
        {
            return true;
        }

        /// <summary>
        /// Executes a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <returns>The number of rows that are affected</returns>
        public int ExecuteNonQuery(SqlCommand sqlCommand)
        {
            sqlCommand = CheckForSQLConnection(sqlCommand);

            _sqlConnection.Open();

            int result = sqlCommand.ExecuteNonQuery();

            _sqlConnection.Close();

            return result;
        }

        /// <summary>
        /// Tries to execute a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The number of rows that are affected</returns>
        public int TryExecuteNonQuery(SqlCommand sqlCommand, out SqlException sqlEx, out Exception ex)
        {
            int result = -1;
            sqlEx = null!;
            ex = null!;

            try
            {
                result = ExecuteNonQuery(sqlCommand);
            }
            catch (SqlException e)
            {
                sqlEx = e;
            }
            catch (Exception e)
            {
                ex = e;
            }

            _sqlConnection.Close();

            return result!;
        }

        /// <summary>
        /// Tries to execute a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <returns>The number of rows that are affected</returns>
        public int TryExecuteNonQuery(SqlCommand sqlCommand, out SqlException sqlEx)
        {
            return TryExecuteNonQuery(sqlCommand, out sqlEx, out _);
        }

        /// <summary>
        /// Tries to execute a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The number of rows that are affected</returns>
        public int TryExecuteNonQuery(SqlCommand sqlCommand, out Exception ex)
        {
            return TryExecuteNonQuery(sqlCommand, out _, out ex);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object ExecuteScalar(SqlCommand sqlCommand)
        {
            sqlCommand = CheckForSQLConnection(sqlCommand);

            _sqlConnection.Open();

            object result = sqlCommand.ExecuteScalar();

            _sqlConnection.Close();

            return result;
        }

        /// <summary>
        /// Trys to execute the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object TryExecuteScalar(SqlCommand sqlCommand, out SqlException sqlEx, out Exception ex)
        {
            object result = null!;
            sqlEx = null!;
            ex = null!;

            try
            {
                result = ExecuteScalar(sqlCommand);
            }
            catch (SqlException e)
            {
                sqlEx = e;
            }
            catch (Exception e)
            {
                ex = e;
            }

            _sqlConnection.Close();

            return result!;
        }

        /// <summary>
        /// Trys to execute the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object TryExecuteScalar(SqlCommand sqlCommand, out SqlException sqlEx)
        {
            return TryExecuteScalar(sqlCommand, out sqlEx, out _);
        }

        /// <summary>
        /// Trys to execute the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object TryExecuteScalar(SqlCommand sqlCommand, out Exception ex)
        {
            return TryExecuteScalar(sqlCommand, out _, out ex);
        }

        private static bool ReturnCheckTable(SqlConnection connect)
        {
            //return CheckTable(connect, "CDate", "Id", "Date") && CheckTable(connect, "Coin", "Id", "Name", "Original Value", "Retail Value", "Amount Made", "ImagePath", "Description") 
            return CheckTable(connect, "CDate", "Id", "Date") && CheckTable(connect, "Coin", "Id", "Name", "Description", "Amount Made", "Currency Type", "Original Value", "Retail Value", "ImagePath") 
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
                            //MessageBox.Show($"{connect.Database} does not have {colomName} colom in {tableName}!!!", "Error");
                            
                            App.GetInstance().Report.ShowMessage($"{connect.Database} does not have {colomName} colom in {tableName}", "Warning", ReportSeverity.Warning);

                            connect.Close();
                            return false;
                        }
                    }
                }

                connect.Close();
                return true;
            }

            //MessageBox.Show($"{connect.Database} does not have {tableName}!!!", "Error");
            
            App.GetInstance().Report.ShowMessage($"{connect.Database} does not have {tableName}", "Warning", ReportSeverity.Warning);

            connect.Close();
            return false;
        }

        private SqlCommand CheckForSQLConnection(SqlCommand sqlCommand)
        {
            if (sqlCommand == null)
            {
                throw new NullReferenceException("SQLCommand is null");
            }

            if (sqlCommand.Connection == null)
            {
                if (_sqlConnection == null)
                {
                    throw new NullReferenceException("No SQL Connection in SQL command or in SQL Container!!!");
                }

                sqlCommand.Connection = _sqlConnection;
            }

            return sqlCommand;
        }

        private bool UpdateJsonSettingsFile(string loc)
        {
            if(string.IsNullOrEmpty(loc))
            {
                return false;
            }

            //TODO: Sort out JSON stuff
            /*var json = JsonNode.Parse(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));

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
                //MessageBox.Show("Unable to open appsettings!!!");

                App.GetInstance().Report.ShowMessage("Unable to open appsettings", "Warning", ReportSeverity.Warning);
                App.GetInstance().GetService<ServerSelectorWindow>().CloseWindow();

                return false;
            }

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));*/

            App appInstance = App.GetInstance();

            if(appInstance.ConfigEditor.ConfigFileExist)
            {
                if (!appInstance.ConfigEditor.Set("SQL Dir", loc) || !appInstance.ConfigEditor.Set("DefaultConnection", $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={loc};Integrated Security=True;Connect Timeout=30", "ConnectionStrings"))
                {
                    appInstance.GetService<ServerSelectorWindow>().CloseWindow();
                    return false;
                }
            }
            else
            {
                appInstance.Report.ShowMessage("Unable to open appsettings", "Warning", ReportSeverity.Warning);
                appInstance.GetService<ServerSelectorWindow>().CloseWindow();

                return false;
            }

            appInstance.ConfigEditor.UpdateConfigFile();

            //App.GetInstance().ConfigWait.WaitOne();
            appInstance.ConfigWait.WaitOne();

            //_sqlConnection.ConnectionString = App.GetInstance().ConnectionString;
            _sqlConnection.ConnectionString = appInstance.ConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<LocalDBMSSQLLocalDBContext>();

            //optionsBuilder.UseSqlServer(App.GetInstance().ConnectionString);
            optionsBuilder.UseSqlServer(appInstance.ConnectionString);

            _localDBContext = new(optionsBuilder.Options);

            return true;
        }
    }
}
