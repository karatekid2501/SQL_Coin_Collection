using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CoinCollection
{
    /// <summary>
    /// Types of operators that can be used in an SQL command
    /// </summary>
    public enum OperatorTypes
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Module,
        Bitwise_AND,
        Bitwise_OR,
        Bitwise_Exclusive_OR,
        Equal_To,
        Greater_Than,
        Less_Than,
        Greater_Than_Or_Equal_To,
        Less_Than_Or_Equal_To,
        Not_Equal_To,
        Add_Equals,
        Subtract_Equals,
        Multiply_Equals,
        Divide_Equals,
        Module_Equals,
        Bitwise_AND_Equals,
        Bitwise_Exclusive_Equals,
        Bitwise_OR_Equals
    }

    public enum FileSizeType
    {
        KB,
        MB,
        GB
    }

    public enum SelectType
    {
        Empty,
        All,
        Name,
        Value
    }

    public struct SetCommand(string nameParam, object value)
    {
        public string NameParam { get; set; } = nameParam;

        public object Value { get; set; } = value;
    }

    public abstract class InservtValuesBase(string name)
    {
        public string Name { get; set; } = name;

        public abstract object Value();
    }

    public class InsertValues<T>(string name, T value) : InservtValuesBase(name) where T : notnull
    {
        private readonly T _value = value;

        public override object Value()
        {
            return _value;
        }
    }

    public class FileSize
    {
        public string Size {  get; private set; }

        public FileSize()
        {
            Size = "UNLIMITED";
        }

        public FileSize(int size, FileSizeType fst = FileSizeType.MB)
        {
            Size = $"{size}{fst}";
        }
    }

    /// <summary>
    /// Simple way to build SQL commands
    /// TODO: Finish off commands
    /// </summary>
    internal class SQLCommandFactory
    {
        //The SQL command to build the command string
        private readonly StringBuilder _sqlCommand = new();

        //Parameters for the SQL command
        private readonly List<SqlParameter> _sqlParameters = [];


        /*public SQLCommandFactory Select(string columnName = "*", bool asValue = false)
        {
            if(string.IsNullOrEmpty(columnName))
            {
                _sqlCommand.Append($"SELECT ");
            }
            else
            {
                if(asValue)
                {
                    _sqlCommand.Append($"SELECT {columnName} ");
                }
                else
                {
                    _sqlCommand.Append($"SELECT '{columnName}' ");
                }
            }

            return this;
        }*/

        /// <summary>
        /// The Select SQL command for selecting columns
        /// </summary>
        /// <param name="selectType">Type of select command</param>
        /// <param name="value">Used for both SelectType.Name and SelectType.Value</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Select(SelectType selectType = SelectType.All, string value = "")
        {
            if (selectType == SelectType.Empty)
            {
                _sqlCommand.Append($"SELECT ");
            }
            else if (selectType == SelectType.All)
            {
                _sqlCommand.Append($"SELECT * ");
            }
            else if (selectType == SelectType.Name)
            {
                _sqlCommand.Append($"SELECT {value} ");
            }
            else
            {
                Select_Value(value);
            }

            return this;
        }

        /// <summary>
        /// The Select SQL command for selecting values
        /// </summary>
        /// <typeparam name="T">Type of parameter value</typeparam>
        /// <param name="value">Value of the parameter</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Select_Value<T>(T value) where T : notnull
        {
            /*string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"SELECT {pName} ");
            _sqlParameters.Add(new SqlParameter(pName, value));*/

            AddParam(value, "SELECT ", " ");

            return this;
        }

        /// <summary>
        /// The Update SQL command
        /// </summary>
        /// <param name="columnName">Coloumn name to select</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Update(string columnName)
        {
            _sqlCommand.Append($"UPDATE {columnName} ");
            
            return this;
        }

        /// <summary>
        /// The Delete SQL command
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Delete()
        {
            _sqlCommand.Append("DELETE ");

            return this;
        }

        /// <summary>
        /// The Insert Into SQL command
        /// </summary>
        /// <param name="columnName">Column name to select</param>
        /// <param name="values">Values to modify in the selected column</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        /// <exception cref="ArgumentException"></exception>
        public SQLCommandFactory Insert_Into(string columnName, params string[] values)
        {
            if(values.Length == 0)
            {
                throw new ArgumentException("Insert into values are not set!!!");
            }

            _sqlCommand.Append($"INSERT INTO {columnName} VALUES (");

            foreach(string value in values)
            {
                /*string pName = $"@param{_sqlParameters.Count}";
                _sqlCommand.Append($"{pName}, ");
                _sqlParameters.Add(new SqlParameter(pName, value));*/

                AddParam(value, string.Empty, ", ");
            }

            _sqlCommand.Remove(_sqlCommand.Length - 2, 2);

            _sqlCommand.Append(") ");

            return this;
        }

        public SQLCommandFactory Insert_Into(string tableName, params InservtValuesBase[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("Insert into values are not set!!!");
            }

            _sqlCommand.Append($"INSERT INTO {tableName} (");

            for(int i = 0; i < values.Length; i++)
            {
                _sqlCommand.Append($"{values[i].Name}");

                if (i != values.Length - 1)
                {
                    _sqlCommand.Append(", ");
                }
            }

            _sqlCommand.Append(") VALUES (");

            for (int i = 0; i < values.Length; i++)
            {
                AddParam(values[i].Value(), string.Empty);

                if (i != values.Length - 1)
                {
                    _sqlCommand.Append(", ");
                }
            }

            _sqlCommand.Append(") ");

            return this;
        }

        public SQLCommandFactory Alter_Database()
        {
            return this;
        }

        public SQLCommandFactory Drop_Database(string dataBaseName)
        {
            _sqlCommand.Append($"DROP DATABASE [{dataBaseName}] ");

            return this;
        }

        public SQLCommandFactory Create_Index()
        {
            return this;
        }

        public SQLCommandFactory Drop_Index()
        {
            return this;
        }

        public SQLCommandFactory Primary_Key()
        {
            return this;
        }

        public SQLCommandFactory Create_Database(string databaseName)
        {
            _sqlCommand.Append($"CREATE DATABASE {databaseName}; ");

            return this;
        }

        public SQLCommandFactory Create_Database(string databaseName, string name)
        {
            return Create_Database(databaseName, name, $"{Path.Combine(Directory.GetCurrentDirectory(), name)}", new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName)
        {
            return Create_Database(databaseName, name, fileName, new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName, FileSize size)
        {
            return Create_Database(databaseName, name, fileName, size, new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName, FileSize size, FileSize maxSize)
        {
            return Create_Database(databaseName, name, fileName, size, maxSize, new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName, FileSize size, FileSize maxSize, FileSize filegrowth)
        {
            _sqlCommand.Append($"CREATE DATABASE {databaseName} ON PRIMARY (");

            //AddParam(name, "NAME = ", ", ");
            _sqlCommand.Append($"NAME = {name}, ");

            if (!fileName.EndsWith(".mdf"))
            {
                fileName += ".mdf";
            }

            //AddParam(fileName, "FILENAME = ", ", ");
            _sqlCommand.Append($"FILENAME = '{fileName}', ");

            _sqlCommand.Append($"SIZE = {size.Size}, ");
            _sqlCommand.Append($"MAXSIZE = {maxSize.Size}, ");
            _sqlCommand.Append($"FILEGROWTH = {filegrowth.Size}) ");

            return this;
        }

        public SQLCommandFactory Log_On(string name)
        {
            return Log_On(name, $"{Path.Combine(Directory.GetCurrentDirectory(), name)}", new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Log_On(string name, string fileName)
        {
            return Log_On(name, fileName, new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Log_On(string name, string fileName, FileSize size)
        {
            return Log_On(name, fileName, size, new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Log_On(string name, string fileName, FileSize size, FileSize maxSize)
        {
            return Log_On(name, fileName, size, maxSize, new FileSize(64, FileSizeType.KB));
        }

        public SQLCommandFactory Log_On(string name, string fileName, FileSize size, FileSize maxSize, FileSize filegrowth)
        {
            _sqlCommand.Append($"LOG ON (");

            //AddParam(name, "NAME = ", ", ");
            _sqlCommand.Append($"NAME = {name}, ");

            if (!fileName.EndsWith(".ldf"))
            {
                fileName += ".ldf";
            }

            //AddParam(fileName, "FILENAME = ", ", ");
            //_sqlCommand.Append($"FILENAME = {fileName}, ");
            _sqlCommand.Append($"FILENAME = '{fileName}', ");

            _sqlCommand.Append($"SIZE = {size.Size}, ");
            _sqlCommand.Append($"MAXSIZE = {maxSize.Size}, ");
            _sqlCommand.Append($"FILEGROWTH = {filegrowth.Size}) ");

            return this;
        }

        public SQLCommandFactory Create_Table(string table)
        {
            _sqlCommand.Append($"CREATE TABLE {table}; ");

            return this;
        }

        /// <summary>
        /// The And SQL command
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory And()
        {
            _sqlCommand.Append("AND ");

            return this;
        }

        /// <summary>
        /// The Not SQL command
        /// </summary>
        /// <param name="condition">Value to check in the SQL server</param>
        /// <param name="value">Value to compare</param>
        /// <param name="oTypes">Operator Types</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Not(string condition, object value, OperatorTypes oTypes = OperatorTypes.Equal_To)
        {
            /*string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"NOT {condition} {OperatorConvert(oTypes)} {pName} ");
            _sqlParameters.Add(new SqlParameter(pName, value));*/

            AddParam(value, $"NOT {condition} {OperatorConvert(oTypes)} ", " ");

            return this;
        }

        public SQLCommandFactory As(string value)
        {
            _sqlCommand.Append($"AS {value} ");

            return this;
        }

        /// <summary>
        /// The From SQL command
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory From(string tableName)
        {
            _sqlCommand.Append($"FROM {tableName} ");

            return this;
        }

        /// <summary>
        /// The Where SQL command
        /// </summary>
        /// <param name="condition">Value to check in the SQL server</param>
        /// <param name="value">Value to compare</param>
        /// <param name="oTypes">Operator Types</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Where(string condition, object value, OperatorTypes oTypes = OperatorTypes.Equal_To)
        {
            /*string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"WHERE {condition} {OperatorConvert(oTypes)} {pName} ");
            _sqlParameters.Add(new SqlParameter(pName, value));*/

            AddParam(value, $"WHERE {condition} {OperatorConvert(oTypes)} ", " ");

            return this;
        }

        public SQLCommandFactory AVG()
        {
            return this;
        }

        public SQLCommandFactory Between()
        {
            return this;
        }

        public SQLCommandFactory Case()
        {
            return this;
        }

        /*public SQLCommandFactory Count(string number = "*")
        {
            if (string.IsNullOrEmpty(number) || number == "*")
            {
                string pName = $"@param{_sqlParameters.Count}";
                _sqlCommand.Append($"COUNT ({pName}) ");
                _sqlParameters.Add(new SqlParameter(pName, number));
            }
            else if (int.TryParse(number, out int numberInt))
            {
                string pName = $"@param{_sqlParameters.Count}";
                _sqlCommand.Append($"COUNT ({pName}) ");
                _sqlParameters.Add(new SqlParameter(pName, numberInt));
            }
            else
            {
                throw new Exception($"{number} is not a number!!!");
            }

            return this;
        }*/

        /// <summary>
        /// The Count SQL command
        /// </summary>
        /// <param name="value">Value to count from</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Count(string value = "*")
        {
            AddParam(value, "COUNT (", ") ");

            //_sqlCommand.Append($"COUNT ({value}) ");

            return this;
        }

        public SQLCommandFactory Group_By()
        {
            return this;
        }

        public SQLCommandFactory Having()
        {
            return this;
        }

        public SQLCommandFactory Inner_Join()
        {
            return this;
        }

        public SQLCommandFactory Insert()
        {
            return this;
        }

        public SQLCommandFactory Is_Null()
        {
            return this;
        }

        /// <summary>
        /// The Is Not Null SQL command
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Is_Not_Null()
        {
            _sqlCommand.Append("IS NOT NULL ");

            return this;
        }

        public SQLCommandFactory Like()
        {
            return this;
        }

        public SQLCommandFactory Limit()
        {
            return this;
        }

        public SQLCommandFactory Max()
        {
            return this;
        }

        public SQLCommandFactory Min()
        {
            return this;
        }

        public SQLCommandFactory Or()
        {
            return this;
        }

        public SQLCommandFactory Order_By()
        {
            return this;
        }

        public SQLCommandFactory Outer_Join()
        {
            return this;
        }

        public SQLCommandFactory Round()
        {
            return this;
        }

        public SQLCommandFactory Select_Distinct()
        {
            return this;
        }

        public SQLCommandFactory Sum()
        {
            return this;
        }

        public SQLCommandFactory With()
        {
            return this;
        }

        /// <summary>
        /// The Else SQL command
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Else()
        {
            _sqlCommand.Append("ELSE ");

            return this;
        }

        /// <summary>
        /// The Col Length SQL command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory COL_LENGTH(string table, string column)
        {
            _sqlCommand.Append("COL_LENGTH (");

            /*string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"{pName}, ");
            _sqlParameters.Add(new SqlParameter(pName, table));

            pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"{pName}");
            _sqlParameters.Add(new SqlParameter(pName, column));*/

            AddParam(table, string.Empty, ", ");
            AddParam(column);

            _sqlCommand.Append(") ");

            return this;
        }

        /// <summary>
        /// The If SQL command
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory If()
        {
            _sqlCommand.Append("IF ");

            return this;
        }

        /// <summary>
        /// The Exist SQL command
        /// </summary>
        /// <param name="command">Command to use in the Exist brackets</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Exists(SQLCommandFactory command)
        {
            string temp = command._sqlCommand.ToString();

            string[] words = temp.Split(["@param"], StringSplitOptions.RemoveEmptyEntries);

            _sqlCommand.Append("EXISTS (");

            for(int i = 0; i < words.Length; i += 2)
            {
                string pName = $"@param{_sqlParameters.Count}";
                _sqlCommand.Append($"{words[i]}{pName}");

                SqlParameter sqlParameter = command._sqlParameters[int.Parse(words[i + 1][0].ToString())];
                sqlParameter.ParameterName = pName;

                _sqlParameters.Add(sqlParameter);
            }

            _sqlCommand.Append(") ");

            return this;
        }

        /// <summary>
        /// The Use SQL command
        /// </summary>
        /// <param name="dataBaseName">Name of database to use</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Use(string dataBaseName)
        {
            _sqlCommand.Append($"Use {dataBaseName}; ");

            return this;
        }

        /// <summary>
        /// The Set SQL command
        /// </summary>
        /// <param name="setInfo">List of values to set</param>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory Set(params SetCommand[] setInfo)
        {
            _sqlCommand.Append("SET ");

            foreach (SetCommand item in setInfo)
            {
                string pName = $"@param{_sqlParameters.Count}";
                _sqlCommand.Append($"[{item.NameParam}] = {pName}, ");
                _sqlParameters.Add(new (pName, item.Value));
            }

            _sqlCommand.Remove(_sqlCommand.Length - 2, 1);

            return this;
        }

        /// <summary>
        /// Ends the command with a comma
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory</returns>
        public SQLCommandFactory EndCommand()
        {
            _sqlCommand.Append("; ");

            return this;
        }

        public SQLCommandFactory Custom(string customString)
        {
            _sqlCommand.Append($"{customString} ");

            return this;
        }

        public SQLCommandFactory Comna(bool removeSpace = true)
        {
            if(removeSpace && _sqlCommand.ToString().EndsWith(" "))
            {
                //https://stackoverflow.com/questions/23626703/stringbuilder-find-last-index-of-a-character
                _sqlCommand.Length--;
            }

            _sqlCommand.Append(", ");

            return this;
        }

        /// <summary>
        /// Converts the SQL command from the string to an SQL command
        /// </summary>
        /// <param name="connection">SQLConnection to be used if avalible</param>
        /// <param name="endWithSemiColon">Ends the string SQL command with a semicolon</param>
        /// <param name="clearCommand">Clears the string and paramiters</param>
        /// <returns>The created SQL command</returns>
        public SqlCommand ToSQLCommand(SqlConnection? connection = null, bool endWithSemiColon = true, bool clearCommand = true)
        {
            _sqlCommand.Remove(_sqlCommand.Length - 1, 1);

            if (endWithSemiColon)
            {
                _sqlCommand.Append(';');
            }

            SqlCommand sqlCommand = new (_sqlCommand.ToString(), connection);

            sqlCommand.Parameters.AddRange([.. _sqlParameters]);

            if(clearCommand)
            {
                _sqlParameters.Clear();
                _sqlCommand.Clear();
            }

            return sqlCommand;
        }

        /// <summary>
        /// Converts the SQL command from the string to an SQL data adapter
        /// </summary>
        /// <param name="connection">SQLConnection to be used if avalible</param>
        /// <param name="endWithSemiColon">Ends the string SQL command with a semicolon</param>
        /// <param name="clearCommand">Clears the string and paramiters</param>
        /// <returns>The created SQL data adapter</returns>
        public SqlDataAdapter ToSQLDataAdapter(SqlConnection? connection = null, bool endWithSemiColon = true, bool clearCommand = true)
        {
            return new SqlDataAdapter(ToSQLCommand(connection, endWithSemiColon, clearCommand));
        }

        /// <summary>
        /// Converts the operator types to their string counterparts
        /// </summary>
        /// <param name="oTypes">Operator types</param>
        /// <returns>Converted operator type</returns>
        /// <exception cref="Exception"></exception>
        private static string OperatorConvert(OperatorTypes oTypes)
        {
            return oTypes switch
            {
                OperatorTypes.Add => "+",
                OperatorTypes.Subtract => "-",
                OperatorTypes.Multiply => "*",
                OperatorTypes.Divide => "/",
                OperatorTypes.Module => "%",
                OperatorTypes.Bitwise_AND => "&",
                OperatorTypes.Bitwise_OR => "|",
                OperatorTypes.Bitwise_Exclusive_OR => "^",
                OperatorTypes.Equal_To => "=",
                OperatorTypes.Greater_Than => ">",
                OperatorTypes.Less_Than => "<",
                OperatorTypes.Greater_Than_Or_Equal_To => ">=",
                OperatorTypes.Less_Than_Or_Equal_To => "<=",
                OperatorTypes.Not_Equal_To => "<>",
                OperatorTypes.Add_Equals => "+=",
                OperatorTypes.Subtract_Equals => "-=",
                OperatorTypes.Multiply_Equals => "*=",
                OperatorTypes.Divide_Equals => "/=",
                OperatorTypes.Module_Equals => "%=",
                OperatorTypes.Bitwise_AND_Equals => "&=",
                OperatorTypes.Bitwise_Exclusive_Equals => "^-=",
                OperatorTypes.Bitwise_OR_Equals => "|*=",
                _ => throw new Exception($"Unknown operator [{oTypes}]"),
            };
        }

        /// <summary>
        /// Adds a parameter to the list of SQL parameters unless its a floating point value (float, double, decimal) due to floating point error, which is added to the command differently
        /// </summary>
        /// <typeparam name="T">Type of parameter value</typeparam>
        /// <param name="value">Value of the parameter</param>
        /// <param name="startAppend">Additional command structure before inserting the value name into the command</param>
        /// <param name="endAppend">Additional command structure after inserting the value name into the command</param>
        private void AddParam(object value, string startAppend = "", string endAppend = "")
        {
            if(value is float valueF)
            {
                _sqlCommand.Append($"{startAppend}{valueF}{endAppend}");
            }
            else if (value is double valueD)
            {
                _sqlCommand.Append($"{startAppend}{valueD}{endAppend}");
            }
            else if (value is decimal valueDec)
            {
                _sqlCommand.Append($"{startAppend}{valueDec}{endAppend}");
            }
            else
            {
                AddParam<object>(value, startAppend, endAppend);
            }
        }

        /// <summary>
        /// Adds a parameter to the list of SQL parameters
        /// </summary>
        /// <typeparam name="T">Type of parameter value</typeparam>
        /// <param name="value">Value of the parameter</param>
        /// <param name="startAppend">Additional command structure before inserting the value name into the command</param>
        /// <param name="endAppend">Additional command structure after inserting the value name into the command</param>
        private void AddParam<T>(T value, string startAppend = "", string endAppend = "") where T : notnull
        {
            string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"{startAppend}{pName}{endAppend}");
            _sqlParameters.Add(new SqlParameter(pName, value));
        }
    }
}
