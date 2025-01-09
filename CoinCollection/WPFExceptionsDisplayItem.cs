using System.Windows.Media;

namespace CoinCollection
{
    internal abstract class WPFExceptionsDisplayItem<T> where T : Exception
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        public Color TextColour { get; private set; }

        public Color BackgroundColour { get; private set; }

        public WPFExceptionsDisplayItem(T item, string name) : this(item, name, Colors.Black, Colors.Transparent) { }

        public WPFExceptionsDisplayItem(T item, string name, Color textColour, Color backgroundColour)
        {
            Name = name;

            Description = Info(item);

            TextColour = textColour;

            BackgroundColour = backgroundColour;
        }

        protected abstract string Info(T item);
    }
}
