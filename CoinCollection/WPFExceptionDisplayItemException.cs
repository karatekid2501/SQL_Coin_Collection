// <copyright file="WPFExceptionDisplayItemException.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Windows.Media;

namespace CoinCollection
{
    /// <summary>
    /// Displays a basic exception.
    /// </summary>
#pragma warning disable S2166 // Classes named like "Exception" should extend "Exception" or a subclass
    internal class WPFExceptionDisplayItemException : WPFExceptionsDisplayItem<Exception>
#pragma warning restore S2166 // Classes named like "Exception" should extend "Exception" or a subclass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WPFExceptionDisplayItemException"/> class.
        /// </summary>
        /// <param name="item">Exception item.</param>
        /// <param name="name">Name of exception.</param>
        public WPFExceptionDisplayItemException(Exception item, string name)
            : base(item, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WPFExceptionDisplayItemException"/> class.
        /// </summary>
        /// <param name="item">Exception item.</param>
        /// <param name="name">Name of exception.</param>
        /// <param name="textColour">Text colour of the exception.</param>
        /// <param name="backgroundColour">Background colour of the exception.</param>
        public WPFExceptionDisplayItemException(Exception item, string name, Color textColour, Color backgroundColour)
            : base(item, name, textColour, backgroundColour)
        {
        }

        /// <inheritdoc/>
        protected override string Info(Exception item)
        {
            return item.Message;
        }
    }
}
