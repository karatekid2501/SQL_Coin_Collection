// <copyright file="StringExtenions.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

namespace CoinCollection
{
    /// <summary>
    /// Cleans up string.
    /// </summary>
    public enum StringCleanUp
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        None,
        Trim,
        TrimStart,
        TrimEnd,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtenions
    {
        /// <summary>
        /// Goes through the string and gets the string after a certain amount of the charCheck is checked.
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <param name="charCheck">Character to check.</param>
        /// <param name="amount">How many positions to check by.</param>
        /// <param name="scu">Type of string cleanup.</param>
        /// <returns>Altered string unless no cherCheck can be found, which original string is returned.</returns>
        public static string IndexOfPos(this string s, char charCheck, int amount, StringCleanUp scu = StringCleanUp.None)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            int currentPos = 0;

            string currentString = s;

            string tempString;

            do
            {
                tempString = currentString.Substring(currentString.IndexOf(charCheck) + 1);

                if (currentString.Length != tempString.Length)
                {
                    currentString = tempString;
                }
                else
                {
                    Console.WriteLine($"{s} does not have '{charCheck}' this {amount}. Only {currentPos}!!!");
                    break;
                }

                currentPos++;
            }
            while (!string.IsNullOrEmpty(currentString) && currentPos != amount);

            if (scu != StringCleanUp.None)
            {
                if (scu == StringCleanUp.Trim)
                {
                    currentString = currentString.Trim();
                }
                else if (scu == StringCleanUp.TrimStart)
                {
                    currentString = currentString.TrimStart();
                }
                else
                {
                    currentString = currentString.TrimEnd();
                }
            }

            return currentString;
        }
    }
}
