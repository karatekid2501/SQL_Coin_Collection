using System.Windows.Media;

namespace CoinCollection
{
    internal class WPFExceptionDisplayItemException : WPFExceptionsDisplayItem<Exception>
    {
        public WPFExceptionDisplayItemException (Exception item, string name) : base (item, name) { }

        public WPFExceptionDisplayItemException(Exception item, string name, Color textColour, Color backgroundColour) : base (item, name, textColour, backgroundColour)
        {

        }

        protected override string Info(Exception item)
        {
            return item.Message;
        }
    }
}
