using CoinCollection.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
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

    /// <summary>
    /// Contains information about the connection to the SQL server and logic for connecting to an SQL connection
    /// </summary>
    public class SQLContainer
    {
        public readonly float ServerVersion = 0.01f;

        private readonly string _serverVersionDescription = "";

        private LocalDBMSSQLLocalDBContext _localDBContext;

        private readonly int _errorTrys = 5;
        private int _currentErrorTrys = 0;

        private readonly SqlConnection _sqlConnection;

        private readonly string[] _tableCreationCommands = [ "CDate (Id INT IDENTITY(1,1) PRIMARY KEY, Date DATE NOT NULL)",
            "Coin (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50) NOT NULL, Description NVARCHAR(MAX) NOT NULL, [Amount Made] INT NOT NULL, [Currency Type] NVARCHAR(MAX) NOT NULL, [Original Value] NVARCHAR(MAX) NOT NULL, [Retail Value] NVARCHAR(MAX) NOT NULL, ImagePath NVARCHAR(MAX) NOT NULL DEFAULT 'No_Coin_Image.jpg')",
            "CoinDate (Id INT IDENTITY(1,1) PRIMARY KEY, DateId INT NOT NULL, CoinId INT NOT NULL, CONSTRAINT DateFK FOREIGN KEY (DateId) REFERENCES CDate(Id), CONSTRAINT CoinFK FOREIGN KEY (CoinId) REFERENCES Coin(Id))",
            "ServerInfo (VersionId INT IDENTITY(1,1) PRIMARY KEY, VersionNumb FLOAT NOT NULL, Description NVARCHAR(MAX), LastUpdated FLOAT NOT NULL)"];

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
            /*if (File.Exists(Path.Combine(loc, "Coins.mdf")))
            {
                return ExistingServer(Path.Combine(loc, "Coins.mdf"));
            }

            SqlConnection connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

            SQLCommandFactory commandFactory = new SQLCommandFactory().Create_Database("Coins", "Coins", $"{Path.Combine(loc, "Coins.mdf")}").Log_On("Coins_log", $"{Path.Combine(loc, "Coins_log.ldf")}");

            //SqlCommand command = new($"CREATE DATABASE Coins ON PRIMARY (NAME = Coins, FILENAME = '{Path.Combine(loc, "Coins.mdf")}') LOG ON (NAME = Coins_log, FILENAME = '{Path.Combine(loc, "Coins_log.ldf")}');", connect);
            SqlCommand command = commandFactory.ToSQLCommand(connect);*/

            string name = loc.Substring(loc.LastIndexOf("\\") + 1);
            string path = loc.Replace(name, "");

            name = name.Remove(name.LastIndexOf("."));

            if (File.Exists($"{path}{name}_Data.mdf"))
            {
                return ExistingServer($"{path}{name}_Data.mdf");
            }

            SqlConnection connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

            SQLCommandFactory commandFactory = new SQLCommandFactory().Create_Database(name, $"{name}_Data", $"{path}{name}_Data.mdf").Log_On($"{name}_log", $"{path}{name}_log.ldf");

            SqlCommand command = commandFactory.ToSQLCommand(connect);

            try
            {
                using (connect)
                {
                    connect.Open();
                    command.ExecuteNonQuery();

                    foreach(string tableCommand in _tableCreationCommands)
                    {
                        //command.CommandText = $"USE Coins; CREATE TABLE {tableCommand};";

                        //command = commandFactory.Use("Coins").Create_Table($"{tableCommand}").ToSQLCommand(connect, false);
                        
                        command = commandFactory.Use($"{name}").Create_Table($"{tableCommand}").ToSQLCommand(connect, false);
                        command.ExecuteNonQuery();
                    }

                    //command.CommandText = $"USE Coins; INSERT INTO ServerInfo (VersionNumb, Description, LastUpdated) VALUES ({ServerVersion}, '{_serverVersionDescription}', {10.1f})";

                    command = commandFactory.Use($"{name}").Insert_Into("ServerInfo", 
                        new InsertValues<float>("VersionNumb", ServerVersion), 
                        new InsertValues<string>("Description", _serverVersionDescription), 
                        new InsertValues<float>("LastUpdated", 10.1f)).ToSQLCommand(connect);
                    
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if(ex.Message.Contains("already exists"))
                {
                    connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

                    using (connect)
                    {
                        string readyName = ex.Message.Substring(ex.Message.IndexOf('\'') + 1, ex.Message.LastIndexOf('\'')).Replace("\' already", "").Trim();

                        //SqlCommand temp = commandFactory.Select(SelectType.Name, "name").As("FileType").Comna().Custom("physical_name").As("FilePath").Comna()
                        //.Custom("type_desc").As("FileTypeDesc").From("sys.master_files").Custom($"WHERE database_id = DB_ID('{readyName}')")
                        //.ToSQLCommand(connect);

                        SqlCommand temp = commandFactory.Select(SelectType.Name, "physical_name").From("sys.master_files").
                            Custom($"WHERE database_id = DB_ID('{readyName}') AND type = 0").ToSQLCommand(connect);

                        connect.Open();

                        string filePath = (string)temp.ExecuteScalar();

                        Debug.WriteLine(filePath);
                    }

                }

                /*_currentErrorTrys++;

                if(_currentErrorTrys == _errorTrys)
                {
                    _currentErrorTrys = 0;

                    App.GetInstance().Report.ShowMessage($"Too many attempts to fix error ({ex.Message})", "Warning", ReportSeverity.Warning);
                    return false;
                }
                else
                {
                    //TODO: Sort out
                    return NewServer(loc);
                }*/

                /*if(ex.Message.Contains("already exists"))
                {
                    connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

                    using (connect)
                    {
                        connect.Open();

                        //SqlCommand clearSQLServer = new ("IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Coins') DROP DATABASE [Coins];", connect);

                        command = commandFactory.If().Exists(new SQLCommandFactory().Select(SelectType.Name, "name").From("sys.databases").Where("name", "Coins")).Drop_Database("Coins").ToSQLCommand(connect);
                        
                        try
                        {
                            //clearSQLServer.ExecuteNonQuery();
                            command.ExecuteNonQuery();
                        }
                        catch (SqlException clearServerErrorCode)
                        {
                            if(!clearServerErrorCode.Message.Contains("Unable to open the physical file"))
                            {
                                App.GetInstance().Report.ShowMessage(clearServerErrorCode, "Warning", ReportSeverity.Warning);

                                return false;
                            }
                        }
                    }

                    return NewServer(loc);
                }
                else
                {
                    App.GetInstance().Report.ShowMessage(ex, "Warning", ReportSeverity.Warning);

                    return false;
                }*/
            }

            //return UpdateJsonSettingsFile(Path.Combine(loc, "Coins.mdf"));
            return UpdateJsonSettingsFile($"{path}{name}_Data.mdf");
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
                App.GetInstance().Report.ShowMessage(ex, "Warning", ReportSeverity.Warning);

                error = SQLError.SQL;
            }
            catch (Exception ex)
            {
                App.GetInstance().Report.ShowMessage(ex, "Warning", ReportSeverity.Warning);

                error = SQLError.Other;
            }
            finally
            {
                _sqlConnection.Close();
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
                //if (!HasConnected(out SQLError error))
                if (!HasConnected())
                {
                    ServerSelectorWindow ssw = App.GetInstance().GetService<ServerSelectorWindow>();

                    ssw.ShowDialog(WindowStartupLocation.CenterScreen, true);

                    if (ssw.IsCancled)
                    {
                        return false;
                    }
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

        /// <summary>
        /// Gets information from the main server
        /// </summary>
        /// <returns>Data from the main server</returns>
        public DataTable GetServerInfo()
        {
            _sqlConnection.Open();

            SqlDataAdapter sqlDataAdapter = new SQLCommandFactory().Select().From("Coin").ToSQLDataAdapter(_sqlConnection);

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

            object result = new SQLCommandFactory().Use("Coins").Select(SelectType.Name, columnName).From(tableName).ToSQLCommand(_sqlConnection).ExecuteScalar();

            _sqlConnection.Close();

            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException($"No data found in {tableName}.{columnName}");
            }

            return (T)Convert.ChangeType(result, typeof(T));
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

        /// <summary>
        /// Checks if the servers have all of the correct information catagories
        /// </summary>
        /// <param name="connect">The connection for the SQL server</param>
        /// <returns>True if the servers have all of the correct information catagories</returns>
        private static bool ReturnCheckTable(SqlConnection connect)
        {
            return CheckTable(connect, "CDate", "Id", "Date") && CheckTable(connect, "Coin", "Id", "Name", "Description", "Amount Made", "Currency Type", "Original Value", "Retail Value", "ImagePath") 
                && CheckTable(connect, "CoinDate", "Id", "DateId", "CoinId") && CheckTable(connect, "ServerInfo", "VersionId", "VersionNumb", "Description", "LastUpdated");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connect"></param>
        /// <param name="tableName"></param>
        /// <param name="colomNames"></param>
        /// <returns></returns>
        private static bool CheckTable(SqlConnection connect, string tableName, params string[] colomNames)
        {
            if(string.IsNullOrEmpty(tableName))
            {
                return false;
            }
            connect.Open();

            if ((int)new SQLCommandFactory().If().Exists(new SQLCommandFactory().Select().From("INFORMATION_SCHEMA.TABLES").Where("TABLE_NAME", tableName)).Select_Value(1).Else().Select_Value(0).ToSQLCommand(connect).ExecuteScalar() == 1)
            {
                if(colomNames.Length != 0)
                {
                    foreach(string colomName in colomNames)
                    {
                        if((int)new SQLCommandFactory().If().COL_LENGTH(tableName, colomName).Is_Not_Null().Select_Value(1).Else().Select_Value(0).ToSQLCommand(connect).ExecuteScalar() == 0)
                        {
                            App.GetInstance().Report.ShowMessage($"{connect.Database} does not have {colomName} colom in {tableName}", "Warning", ReportSeverity.Warning);

                            connect.Close();
                            return false;
                        }
                    }
                }

                connect.Close();
                return true;
            }

            App.GetInstance().Report.ShowMessage($"{connect.Database} does not have {tableName}", "Warning", ReportSeverity.Warning);

            connect.Close();
            return false;
        }

        /// <summary>
        /// Checks if there is a connection to the SQL server and if so, stores it in the SQLCommand
        /// </summary>
        /// <param name="sqlCommand">SQLCommand to check</param>
        /// <returns>The SQLCommand if there is a connection</returns>
        /// <exception cref="NullReferenceException"></exception>
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

        /// <summary>
        /// Updates the Json settings file
        /// </summary>
        /// <param name="loc">Location of the Json settings file</param>
        /// <returns>True if the Json settings file was successfully updated</returns>
        private bool UpdateJsonSettingsFile(string loc)
        {
            if(string.IsNullOrEmpty(loc))
            {
                return false;
            }

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

            appInstance.ConfigWait.WaitOne();

            _sqlConnection.ConnectionString = appInstance.ConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<LocalDBMSSQLLocalDBContext>();

            optionsBuilder.UseSqlServer(appInstance.ConnectionString);

            _localDBContext = new(optionsBuilder.Options);

            return true;
        }
    }
}
