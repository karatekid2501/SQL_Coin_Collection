// <copyright file="ServerDataContainer.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Data;

namespace CoinCollection
{
    /// <summary>
    /// Contains information about the coin.
    /// </summary>
    /// <param name="name">Name of the coin.</param>
    /// <param name="description">Description of the coin.</param>
    /// <param name="dateOfCreation">Date of the creation of the coin.</param>
    /// <param name="amountMade">Coin amount.</param>
    /// <param name="currencyType">Coin currency type.</param>
    /// <param name="originalValue">Original coin value.</param>
    /// <param name="retailValue">Retail coin value.</param>
    /// <param name="image">Image of the coin.</param>
    public class ServerDataContainer(string name, string description, string dateOfCreation, string amountMade, string currencyType,
        string originalValue, string retailValue, string image)
    {
        /// <summary>
        /// Layout of the coin information table.
        /// </summary>
        public readonly string CoinInfo = $"('{name}', '{description}', '{amountMade}', '{currencyType}', '{originalValue}', '{retailValue}', '{image}')";

        /// <summary>
        /// Layout of the date and time table.
        /// </summary>
        public readonly string CoinDateAndTime = $"({dateOfCreation})";

        /// <summary>
        /// 
        /// </summary>
        public readonly bool NotNull = true;

        private readonly string[] _info =
        [
            name,
            description,
            dateOfCreation,
            amountMade,
            currencyType,
            originalValue,
            retailValue,
            image
        ];

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataContainer"/> class.
        /// </summary>
        /// <param name="drv">Data row view for coin info.</param>
        /// <param name="drvDate">Data row view for date table (Not in used).</param>
        public ServerDataContainer(DataRowView drv, DataRowView? drvDate = null)
            : this
            (
            drv?[1]?.ToString() ?? "∅",
            drv?[2]?.ToString() ?? "∅",
            drvDate?[0]?.ToString() ?? "∅",
            drv?[3]?.ToString() ?? "∅",
            drv?[4]?.ToString() ?? "∅",
            drv?[5]?.ToString() ?? "∅",
            drv?[6]?.ToString() ?? "∅",
            drv?[7]?.ToString() ?? "∅") => NotNull = drv != null;

        /// <summary>
        /// Gets information from _info using position.
        /// </summary>
        /// <param name="key">Position in _info to get information from.</param>
        /// <returns>Found information from _info.</returns>
        public string this[int key]
        {
            get
            {
                return _info[key];
            }
        }
    }
}