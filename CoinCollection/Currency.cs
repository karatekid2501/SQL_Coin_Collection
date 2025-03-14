// <copyright file="Currency.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.IO;

namespace CoinCollection
{
    /// <summary>
    /// Currency information.
    /// </summary>
    public partial class Currency
    {
        /// <summary>
        /// Gets name of the currency.
        /// </summary>
        public string CurrencyName { get; private set; }

        /// <summary>
        /// Gets list of currency used from the currency name.
        /// </summary>
        public string[] CurrencyInfo { get; private set; }
    }

    /// <summary>
    /// Currency implementation.
    /// </summary>
    public partial class Currency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Currency"/> class.
        /// </summary>
        public Currency()
        {
            CurrencyName = "Unknown";
            CurrencyInfo = ["Unknown"];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Currency"/> class.
        /// </summary>
        /// <param name="fileLocation">Location of the file.</param>
        public Currency(string fileLocation)
        {
            string[] fileInfo = File.ReadAllText(fileLocation).Split(',');

            List<string> tempCurrencyInfo = ["Unknown"];

            foreach (string info in fileInfo)
            {
                if (info.StartsWith('[') && info.EndsWith(']'))
                {
                    CurrencyName = info[1..info.IndexOf(']')];
                }
                else
                {
                    tempCurrencyInfo.Add(info);
                }
            }

            // Converts the list of currency information to an array
            CurrencyInfo = [.. tempCurrencyInfo];

            // If the currency name is not found within the file, the name is taken from the file name
            if (string.IsNullOrEmpty(CurrencyName))
            {
                CurrencyName = Path.GetFileName(fileLocation);
                CurrencyName = CurrencyName[..CurrencyName.IndexOf('.')];
            }
        }
    }
}
