// <copyright file="SQLCommandFactory.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CoinCollection
{
    /// <summary>
    /// Types of operators that can be used in an SQL command.
    /// </summary>
    public enum OperatorTypes
    {
#pragma warning disable SA1602 // Enumeration items should be documented
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
        Bitwise_OR_Equals,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Size of the value.
    /// </summary>
    public enum FileSizeType
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        KB,
        MB,
        GB,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Selection type for the select method.
    /// </summary>
    public enum SelectType
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        Empty,
        All,
        Name,
        Value,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Used in the Set SQL command to set values in the current SQL database.
    /// </summary>
    /// <param name="nameParam">Name of the parameter to set.</param>
    /// <param name="value">Value to set the parameter to.</param>
    public class SetCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetCommand"/> class.
        /// </summary>
        /// <param name="nameParam">Name of the parameter.</param>
        /// <param name="value">The value to use.</param>
        public SetCommand(string nameParam, object value)
        {
            NameParam = nameParam;

            Value = value;
        }

        private SetCommand()
        {
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string NameParam { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the new value to set the parameter to.
        /// </summary>
        public object Value { get; private set; } = null!;
    }

    /// <summary>
    /// Base class for the insert valus.
    /// </summary>
    /// <param name="name">Parameter name to insert value into.</param>
    public abstract class InsertValuesBase(string name)
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// Value to get from children classes.
        /// </summary>
        /// <returns>The value stored in the children classes.</returns>
        public abstract object Value();
    }

    /// <summary>
    /// Inserts a value into a parameter that is stored in a SQL database table.
    /// </summary>
    /// <typeparam name="T">Type of the value to use.</typeparam>
    /// <param name="name">Parameter name to insert value into.</param>
    /// <param name="value">Value to use to insert using the paramemter.</param>
    public class InsertValues<T>(string name, T value) : InsertValuesBase(name)
        where T : notnull
    {
        private readonly T _value = value;

        /// <inheritdoc/>
        public override object Value()
        {
            return _value;
        }
    }

    /// <summary>
    /// Size of the file.
    /// </summary>
    public class FileSize
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSize"/> class.
        /// </summary>
        public FileSize()
        {
            Size = "UNLIMITED";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSize"/> class.
        /// </summary>
        /// <param name="size">Size of the file.</param>
        /// <param name="fst">File type of size.</param>
        public FileSize(int size, FileSizeType fst = FileSizeType.MB)
        {
            Size = $"{size}{fst}";
        }

        /// <summary>
        /// Gets size of the file.
        /// </summary>
        public string Size { get; private set; }
    }

    /// <summary>
    /// Simple way to build SQL commands.
    /// TODO: Finish off commands.
    /// </summary>
    internal class SQLCommandFactory
    {
        // The SQL command to build the command string
        private readonly StringBuilder _sqlCommand = new();

        // Parameters for the SQL command
        private readonly List<SqlParameter> _sqlParameters = [];

        /// <summary>
        /// The Select SQL command for selecting columns.
        /// </summary>
        /// <param name="selectType">Type of select command.</param>
        /// <param name="value">Used for both SelectType.Name and SelectType.Value.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
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
        /// The Select SQL command for selecting values.
        /// </summary>
        /// <typeparam name="T">Type of parameter value.</typeparam>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Select_Value<T>(T value)
            where T : notnull
        {
            AddParam(value, "SELECT ", " ");

            return this;
        }

        /// <summary>
        /// The Update SQL command.
        /// </summary>
        /// <param name="columnName">Coloumn name to select.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Update(string columnName)
        {
            _sqlCommand.Append($"UPDATE {columnName} ");

            return this;
        }

        /// <summary>
        /// The Delete SQL command.
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Delete()
        {
            _sqlCommand.Append("DELETE ");

            return this;
        }

        /// <summary>
        /// The Insert Into SQL command.
        /// </summary>
        /// <param name="columnName">Column name to select.</param>
        /// <param name="values">Values to modify in the selected column.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        /// <exception cref="ArgumentException">No values were passed.</exception>
        public SQLCommandFactory Insert_Into(string columnName, params string[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("Insert into values are not set!!!");
            }

            _sqlCommand.Append($"INSERT INTO {columnName} VALUES (");

            foreach (string value in values)
            {
                AddParam(value, string.Empty, ", ");
            }

            _sqlCommand.Remove(_sqlCommand.Length - 2, 2);

            _sqlCommand.Append(") ");

            return this;
        }

        /// <summary>
        /// The Insert Into SQL command.
        /// </summary>
        /// <param name="tableName">Table name to select.</param>
        /// <param name="values">Values to modify in the selected table.</param>
        /// <returns>Altered version of SQLCommandFactory (Not implemented).</returns>
        public SQLCommandFactory Insert_Into(string tableName, params InsertValuesBase[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("Insert into values are not set!!!");
            }

            _sqlCommand.Append($"INSERT INTO {tableName} (");

            for (int i = 0; i < values.Length; i++)
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

        /// <summary>
        /// The Alter Database SQL command (Not Implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Alter_Database()
        {
            return this;
        }

        /// <summary>
        /// The Drop Database SQL command.
        /// </summary>
        /// <param name="dataBaseName">Name of the data base to drop (Delete).</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Drop_Database(string dataBaseName)
        {
            _sqlCommand.Append($"DROP DATABASE [{dataBaseName}] ");

            return this;
        }

        /// <summary>
        /// The Create Index SQL command (Not Implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Index()
        {
            return this;
        }

        /// <summary>
        /// The Drop Index SQL command (Not Implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Drop_Index()
        {
            return this;
        }

        /// <summary>
        /// The Create Index SQL command (Not Implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Primary_Key()
        {
            return this;
        }

        /// <summary>
        /// The Create Database SQL command.
        /// </summary>
        /// <param name="databaseName">Name of the data base to create.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Database(string databaseName)
        {
            _sqlCommand.Append($"CREATE DATABASE {databaseName}; ");

            return this;
        }

        /// <summary>
        /// The Create Database SQL command.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="name">Name of the Table.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Database(string databaseName, string name)
        {
            return Create_Database(databaseName, name, $"{Path.Combine(Directory.GetCurrentDirectory(), name)}", new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Create Database SQL command.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="name">Name of the Table.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName)
        {
            return Create_Database(databaseName, name, fileName, new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Create Database SQL command.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="name">Name of the Table.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Starting size of the file.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName, FileSize size)
        {
            return Create_Database(databaseName, name, fileName, size, new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Create Database SQL command.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="name">Name of the Table.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Starting size of the file.</param>
        /// <param name="maxSize">Max size of the file.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName, FileSize size, FileSize maxSize)
        {
            return Create_Database(databaseName, name, fileName, size, maxSize, new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Create Database SQL command.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="name">Name of the Table.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Starting size of the file.</param>
        /// <param name="maxSize">Max size of the file.</param>
        /// <param name="filegrowth">How much the file will grow.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Database(string databaseName, string name, string fileName, FileSize size, FileSize maxSize, FileSize filegrowth)
        {
            _sqlCommand.Append($"CREATE DATABASE [{databaseName}] ON PRIMARY (");

            _sqlCommand.Append($"NAME = [{name}], ");

            if (!fileName.EndsWith(".mdf"))
            {
                fileName += ".mdf";
            }

            _sqlCommand.Append($"FILENAME = '{fileName}', ");

            _sqlCommand.Append($"SIZE = {size.Size}, ");
            _sqlCommand.Append($"MAXSIZE = {maxSize.Size}, ");
            _sqlCommand.Append($"FILEGROWTH = {filegrowth.Size}) ");

            return this;
        }

        /// <summary>
        /// The Log On SQL command.
        /// </summary>
        /// <param name="name">Name of the Log.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Log_On(string name)
        {
            return Log_On(name, $"{Path.Combine(Directory.GetCurrentDirectory(), name)}", new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Log On SQL command.
        /// </summary>
        /// <param name="name">Name of the Log.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Log_On(string name, string fileName)
        {
            return Log_On(name, fileName, new FileSize(8), new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Log On SQL command.
        /// </summary>
        /// <param name="name">Name of the Log.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Size of the file.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Log_On(string name, string fileName, FileSize size)
        {
            return Log_On(name, fileName, size, new FileSize(), new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Log On SQL command.
        /// </summary>
        /// <param name="name">Name of the Log.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Starting size of the file.</param>
        /// <param name="maxSize">Max size of the file.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Log_On(string name, string fileName, FileSize size, FileSize maxSize)
        {
            return Log_On(name, fileName, size, maxSize, new FileSize(64, FileSizeType.KB));
        }

        /// <summary>
        /// The Log On SQL command.
        /// </summary>
        /// <param name="name">Name of the Log.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Starting size of the file.</param>
        /// <param name="maxSize">Max size of the file.</param>
        /// <param name="filegrowth">How much the file will grow.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Log_On(string name, string fileName, FileSize size, FileSize maxSize, FileSize filegrowth)
        {
            _sqlCommand.Append($"LOG ON (");

            _sqlCommand.Append($"NAME = [{name}], ");

            if (!fileName.EndsWith(".ldf"))
            {
                fileName += ".ldf";
            }

            _sqlCommand.Append($"FILENAME = '{fileName}', ");

            _sqlCommand.Append($"SIZE = {size.Size}, ");
            _sqlCommand.Append($"MAXSIZE = {maxSize.Size}, ");
            _sqlCommand.Append($"FILEGROWTH = {filegrowth.Size}) ");

            return this;
        }

        /// <summary>
        /// The Create Table SQL command.
        /// </summary>
        /// <param name="table">Name of the table to create.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Create_Table(string table)
        {
            _sqlCommand.Append($"CREATE TABLE {table}; ");

            return this;
        }

        /// <summary>
        /// The And SQL command.
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory And()
        {
            _sqlCommand.Append("AND ");

            return this;
        }

        /// <summary>
        /// The Not SQL command.
        /// </summary>
        /// <param name="condition">Value to check in the SQL server.</param>
        /// <param name="value">Value to compare.</param>
        /// <param name="oTypes">Operator Types.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Not(string condition, object value, OperatorTypes oTypes = OperatorTypes.Equal_To)
        {
            AddParam(value, $"NOT {condition} {OperatorConvert(oTypes)} ", " ");

            return this;
        }

        /// <summary>
        /// The As SQL command.
        /// </summary>
        /// <param name="value">Value to use.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory As(string value)
        {
            _sqlCommand.Append($"AS {value} ");

            return this;
        }

        /// <summary>
        /// The From SQL command.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory From(string tableName)
        {
            _sqlCommand.Append($"FROM {tableName} ");

            return this;
        }

        /// <summary>
        /// The Where SQL command.
        /// </summary>
        /// <param name="condition">Value to check in the SQL server.</param>
        /// <param name="value">Value to compare.</param>
        /// <param name="oTypes">Operator Types.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Where(string condition, object value, OperatorTypes oTypes = OperatorTypes.Equal_To)
        {
            AddParam(value, $"WHERE {condition} {OperatorConvert(oTypes)} ", " ");

            return this;
        }

        /// <summary>
        /// The AVG SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory AVG()
        {
            return this;
        }

        /// <summary>
        /// The Between SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Between()
        {
            return this;
        }

        /// <summary>
        /// The Case SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Case()
        {
            return this;
        }

        /// <summary>
        /// The Count SQL command.
        /// </summary>
        /// <param name="value">Value to count from.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Count(string value = "*")
        {
            AddParam(value, "COUNT (", ") ");

            return this;
        }

        /// <summary>
        /// The Group By SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Group_By()
        {
            return this;
        }

        /// <summary>
        /// The Is Null SQL command (Not Implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Having()
        {
            return this;
        }

        /// <summary>
        /// The Inner Join SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Inner_Join()
        {
            return this;
        }

        /// <summary>
        /// The Insert SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Insert()
        {
            return this;
        }

        /// <summary>
        /// The Is Null SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Is_Null()
        {
            return this;
        }

        /// <summary>
        /// The Is Not Null SQL command.
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Is_Not_Null()
        {
            _sqlCommand.Append("IS NOT NULL ");

            return this;
        }

        /// <summary>
        /// The Like SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Like()
        {
            return this;
        }

        /// <summary>
        /// The Limit SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Limit()
        {
            return this;
        }

        /// <summary>
        /// The Mex SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Max()
        {
            return this;
        }

        /// <summary>
        /// The Min SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Min()
        {
            return this;
        }

        /// <summary>
        /// The Or SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Or()
        {
            return this;
        }

        /// <summary>
        /// The Order By SQL command (Not implemented).
        /// </summary>
        /// <param name="value">Value to use.</param>
        /// <param name="descending">Should the order be descending.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Order_By(string value, bool descending = true)
        {
            _sqlCommand.Append($"ORDER BY {value} ");

            if (descending)
            {
                _sqlCommand.Append("DESC ");
            }

            return this;
        }

        /// <summary>
        /// The Outer Join SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Outer_Join()
        {
            return this;
        }

        /// <summary>
        /// The Round SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Round()
        {
            return this;
        }

        /// <summary>
        /// The Select Distinct SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Select_Distinct()
        {
            return this;
        }

        /// <summary>
        /// The Sum SQL command (Not implemented).
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Sum()
        {
            return this;
        }

        /// <summary>
        /// The With SQL command.
        /// </summary>
        /// <param name="value">Value to use.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory With(string value)
        {
            _sqlCommand.Append($"WITH {value} ");

            return this;
        }

        /// <summary>
        /// The Else SQL command.
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Else()
        {
            _sqlCommand.Append("ELSE ");

            return this;
        }

        /// <summary>
        /// The Col Length SQL command.
        /// </summary>
        /// <param name="table">Table to use.</param>
        /// <param name="column">Column in the table to use.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory COL_LENGTH(string table, string column)
        {
            _sqlCommand.Append("COL_LENGTH (");

            AddParam(table, string.Empty, ", ");
            AddParam(column);

            _sqlCommand.Append(") ");

            return this;
        }

        /// <summary>
        /// The If SQL command.
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory If()
        {
            _sqlCommand.Append("IF ");

            return this;
        }

        /// <summary>
        /// The Exist SQL command.
        /// </summary>
        /// <param name="command">Command to use in the Exist brackets.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Exists(SQLCommandFactory command)
        {
            string temp = command._sqlCommand.ToString();

            string[] words = temp.Split(["@param"], StringSplitOptions.RemoveEmptyEntries);

            _sqlCommand.Append("EXISTS (");

            for (int i = 0; i < words.Length; i += 2)
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
        /// The Use SQL command.
        /// </summary>
        /// <param name="dataBaseName">Name of database to use.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Use(string dataBaseName)
        {
            if (dataBaseName.Contains(' '))
            {
                dataBaseName = $"[{dataBaseName}]";
            }

            _sqlCommand.Append($"Use {dataBaseName}; ");

            return this;
        }

        /// <summary>
        /// The Set SQL command.
        /// </summary>
        /// <param name="setInfo">List of values to set.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Set(params SetCommand[] setInfo)
        {
            _sqlCommand.Append("SET ");

            foreach (SetCommand item in setInfo)
            {
                string pName = $"@param{_sqlParameters.Count}";
                _sqlCommand.Append($"[{item.NameParam}] = {pName}, ");
                _sqlParameters.Add(new(pName, item.Value));
            }

            _sqlCommand.Remove(_sqlCommand.Length - 2, 1);

            return this;
        }

        /// <summary>
        /// Ends the command with a comma.
        /// </summary>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory EndCommand()
        {
            _sqlCommand.Append("; ");

            return this;
        }

        /// <summary>
        /// Custom command.
        /// </summary>
        /// <param name="customString">Custom command value.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Custom(string customString)
        {
            _sqlCommand.Append($"{customString} ");

            return this;
        }

        /// <summary>
        /// Adds a comma to the SQL command.
        /// </summary>
        /// <param name="removeSpace">Removes the space at the end of the command.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Comma(bool removeSpace = true)
        {
            if (removeSpace && _sqlCommand.ToString().EndsWith(' '))
            {
                // https://stackoverflow.com/questions/23626703/stringbuilder-find-last-index-of-a-character
                _sqlCommand.Length--;
            }

            _sqlCommand.Append(", ");

            return this;
        }

        /// <summary>
        /// The Exec SQL command.
        /// </summary>
        /// <param name="value">Value to execute.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Exec(string value)
        {
            _sqlCommand.Append($"EXEC {value} ");

            return this;
        }

        /// <summary>
        /// Backups the current database.
        /// </summary>
        /// <param name="databaseName">Database to backup.</param>
        /// <param name="path">Path to save database to.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public SQLCommandFactory Backup_Database(string databaseName, string path)
        {
            _sqlCommand.Append($"BACKUP DATABASE [{databaseName}] TO DISK = ");

            AddParam(path, string.Empty, " ");

            return this;
        }

        /// <summary>
        /// Converts the created SQL command into a string.
        /// </summary>
        /// <param name="endWithSemiColon">Ends the string SQL command with a semicolon.</param>
        /// <param name="clearCommand">Clears the string and paramiters.</param>
        /// <returns>Altered version of SQLCommandFactory.</returns>
        public string ToCommandText(bool endWithSemiColon = true, bool clearCommand = true)
        {
            if (_sqlCommand.ToString().EndsWith(' '))
            {
                _sqlCommand.Remove(_sqlCommand.Length - 1, 1);
            }

            if (endWithSemiColon)
            {
                _sqlCommand.Append(';');
            }

            string command = _sqlCommand.ToString();

            foreach (SqlParameter parameter in _sqlParameters)
            {
                command = command.Replace(parameter.ParameterName, ConvertValue(parameter.Value));
            }

            if (clearCommand)
            {
                _sqlParameters.Clear();
                _sqlCommand.Clear();
            }

            return command;
        }

        /// <summary>
        /// Converts the SQL command from the string to an SQL command.
        /// </summary>
        /// <param name="connection">SQLConnection to be used if avalible.</param>
        /// <param name="endWithSemiColon">Ends the string SQL command with a semicolon.</param>
        /// <param name="clearCommand">Clears the string and paramiters.</param>
        /// <returns>The created SQL command.</returns>
        public SqlCommand ToSQLCommand(SqlConnection? connection = null, bool endWithSemiColon = true, bool clearCommand = true)
        {
            if (_sqlCommand.ToString().EndsWith(' '))
            {
                _sqlCommand.Remove(_sqlCommand.Length - 1, 1);
            }

            if (endWithSemiColon)
            {
                _sqlCommand.Append(';');
            }

            SqlCommand sqlCommand = new(_sqlCommand.ToString(), connection);

            sqlCommand.Parameters.AddRange([.. _sqlParameters]);

            if (clearCommand)
            {
                _sqlParameters.Clear();
                _sqlCommand.Clear();
            }

            return sqlCommand;
        }

        /// <summary>
        /// Converts the SQL command from the string to an SQL data adapter.
        /// </summary>
        /// <param name="connection">SQLConnection to be used if avalible.</param>
        /// <param name="endWithSemiColon">Ends the string SQL command with a semicolon.</param>
        /// <param name="clearCommand">Clears the string and paramiters.</param>
        /// <returns>The created SQL data adapter.</returns>
        public SqlDataAdapter ToSQLDataAdapter(SqlConnection? connection = null, bool endWithSemiColon = true, bool clearCommand = true)
        {
            return new SqlDataAdapter(ToSQLCommand(connection, endWithSemiColon, clearCommand));
        }

        /// <summary>
        /// Converts the operator types to their string counterparts.
        /// </summary>
        /// <param name="oTypes">Operator types.</param>
        /// <returns>Converted operator type.</returns>
        /// <exception cref="ArgumentException">Throws when an unknown opertator is added but no value is set for it.</exception>
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
                _ => throw new ArgumentException($"Unknown operator [{oTypes}]"),
            };
        }

        /// <summary>
        /// Converts value to a string with proper format.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="ArgumentNullException">If the value can not be converted to a string.</exception>
        private static string ConvertValue(object value)
        {
            if (value == DBNull.Value)
            {
                return "NULL";
            }

            if (value.ToString() == null)
            {
                throw new ArgumentNullException($"Unable to convert {value} to string!!!");
            }

            return Type.GetTypeCode(value.GetType()) switch
            {
                TypeCode.String or TypeCode.Char => $"'{value}'",
                TypeCode.DateTime => $"'{(DateTime)value}'",
                TypeCode.Boolean => (bool)value ? "1" : "0",
                _ => value.ToString()!,
            };
        }

        /// <summary>
        /// Adds a parameter to the list of SQL parameters unless its a floating point value (float, double, decimal) due to floating point error, which is added to the command differently.
        /// </summary>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="startAppend">Additional command structure before inserting the value name into the command.</param>
        /// <param name="endAppend">Additional command structure after inserting the value name into the command.</param>
        private void AddParam(object value, string startAppend = "", string endAppend = "")
        {
            if (value is float valueF)
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
        /// Adds a parameter to the list of SQL parameters.
        /// </summary>
        /// <typeparam name="T">Type of parameter value.</typeparam>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="startAppend">Additional command structure before inserting the value name into the command.</param>
        /// <param name="endAppend">Additional command structure after inserting the value name into the command.</param>
        private void AddParam<T>(T value, string startAppend = "", string endAppend = "")
            where T : notnull
        {
            string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"{startAppend}{pName}{endAppend}");
            _sqlParameters.Add(new SqlParameter(pName, value));
        }
    }
}
