using Microsoft.Data.SqlClient;
using System.Text;
using System.Windows.Media;

namespace CoinCollection
{
    internal class WPFExceptionDisplayItemSQLException : WPFExceptionsDisplayItem<SqlException>
    {
        public WPFExceptionDisplayItemSQLException(SqlException item, string name) : base(item, name) { }

        public WPFExceptionDisplayItemSQLException(SqlException item, string name, Color textColour, Color backgroundColour) : base(item, name, textColour, backgroundColour)
        {

        }

        protected override string Info(SqlException item)
        {
            StringBuilder sb = new ();

            foreach(SQLError error in item.Errors)
            {
                sb.AppendLine(error.ToString());
            }

            return sb.ToString();
        }
    }
}
