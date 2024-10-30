namespace CoinCollection
{
    public enum StringCleanUp
    {
        None,
        Trim,
        TrimStart,
        TrimEnd
    }

    public static class StringExtenions
    {
        public static string IndexOfPos(this string s, char charCheck, int amount, StringCleanUp scu = StringCleanUp.None)
        {
            if(string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            int currentPos = 0;

            string currentString = s;

            string tempString;

            do
            {
                tempString = currentString.Substring(currentString.IndexOf(charCheck) + 1);

                if(currentString.Length != tempString.Length)
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

            if(scu != StringCleanUp.None)
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
