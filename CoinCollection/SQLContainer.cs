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
using System.Threading.Tasks;

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
            return true;
        }

        public bool NewServer(string loc)
        {
            SqlConnection connect = new(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");

            /*using (connect)
            {
                connect.Open();
                new SqlCommand("drop database Coins", connect).ExecuteNonQuery();
            }*/

            //SqlCommand command = new($@"""CREATE DATABASE Coins ON PRIMARY (NAME = Coins, FILENAME = '{Path.Combine(loc, "Coins.mdf")}') LOG ON (NAME = Coins_log, FILENAME = '{Path.Combine(loc, "Coins_log.ldf")}'); """, connect);
            SqlCommand command = new($"CREATE DATABASE Coins ON PRIMARY (NAME = Coins, FILENAME = '{Path.Combine(loc, "Coins.mdf")}') LOG ON (NAME = Coins_log, FILENAME = '{Path.Combine(loc, "Coins_log.ldf")}');", connect);

            using (connect)
            {
                connect.Open();
                command.ExecuteNonQuery();
            }

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
            if(string.IsNullOrEmpty(_sqlDir) || Directory.Exists(_sqlDir))
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
    }
}
