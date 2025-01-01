using Microsoft.Data.SqlClient;
using System.Text;

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

        public SQLCommandFactory Select(string columnName = "*")
        {
            if(string.IsNullOrEmpty(columnName))
            {
                _sqlCommand.Append($"SELECT ");
            }
            else
            {
                _sqlCommand.Append($"SELECT '{columnName}' ");
            }

            return this;
        }

        public SQLCommandFactory Update()
        {
            return this;
        }

        public SQLCommandFactory Delete()
        {
            _sqlCommand.Append("DELETE ");

            return this;
        }

        public SQLCommandFactory Insert_Into()
        {
            return this;
        }

        public SQLCommandFactory Create_Database()
        {
            return this;
        }

        public SQLCommandFactory Alter_Database()
        {
            return this;
        }

        public SQLCommandFactory Drop_Database()
        {
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

        public SQLCommandFactory And()
        {
            _sqlCommand.Append("AND ");

            return this;
        }

        public SQLCommandFactory Not(string condition, object value, OperatorTypes oTypes = OperatorTypes.Equal_To)
        {
            string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"NOT {condition} {OperatorConvert(oTypes)} {pName} ");
            _sqlParameters.Add(new SqlParameter(pName, value));

            return this;
        }

        public SQLCommandFactory As()
        {
            return this;
        }

        public SQLCommandFactory From(string tableName)
        {
            _sqlCommand.Append($"FROM {tableName} ");

            return this;
        }

        public SQLCommandFactory Where(string condition, object value, OperatorTypes oTypes = OperatorTypes.Equal_To)
        {
            string pName = $"@param{_sqlParameters.Count}";
            _sqlCommand.Append($"WHERE {condition} {OperatorConvert(oTypes)} {pName} ");
            _sqlParameters.Add(new SqlParameter(pName, value));
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

        public SQLCommandFactory Count(string number = "*")
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

        public SQLCommandFactory Is_Not_Null()
        {
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

        public SQLCommandFactory EndCommand()
        {
            _sqlCommand.Append("; ");

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
        /// Converts the operator types to their string counterparts
        /// </summary>
        /// <param name="oTypes">Operator types</param>
        /// <returns>Converted operator type</returns>
        /// <exception cref="Exception">Failsafe if the an operator conversion was forgotten</exception>
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
    }
}
