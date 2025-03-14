// <copyright file="WPFExceptionsDisplayItem.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Windows.Media;

namespace CoinCollection
{
    /// <summary>
    /// Displays exceptions.
    /// </summary>
    /// <typeparam name="T">Type of exception.</typeparam>
    internal abstract class WPFExceptionsDisplayItem<T>
        where T : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WPFExceptionsDisplayItem{T}"/> class.
        /// </summary>
        /// <param name="item">Exception item.</param>
        /// <param name="name">Name of exception.</param>
        protected WPFExceptionsDisplayItem(T item, string name)
            : this(item, name, Colors.Black, Colors.Transparent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WPFExceptionsDisplayItem{T}"/> class.
        /// </summary>
        /// <param name="item">Exception item.</param>
        /// <param name="name">Name of exception.</param>
        /// <param name="textColour">Text colour of the exception.</param>
        /// <param name="backgroundColour">Background colour of the exception.</param>
        protected WPFExceptionsDisplayItem(T item, string name, Color textColour, Color backgroundColour)
        {
            Name = name;

#pragma warning disable S1699 // Constructors should only call non-overridable methods
            Description = Info(item);
#pragma warning restore S1699 // Constructors should only call non-overridable methods

            TextColour = textColour;

            BackgroundColour = backgroundColour;
        }

        /// <summary>
        /// Gets name of exception.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets description of exception.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets text colour of exception.
        /// </summary>
        public Color TextColour { get; private set; }

        /// <summary>
        /// Gets backgroun colour of the exception.
        /// </summary>
        public Color BackgroundColour { get; private set; }

        /// <summary>
        /// Formatted information about the exception.
        /// </summary>
        /// <param name="item">Exception type.</param>
        /// <returns>Information about the exception.</returns>
        protected abstract string Info(T item);
    }
}
