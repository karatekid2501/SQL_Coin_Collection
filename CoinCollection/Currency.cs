using System.IO;

namespace CoinCollection
{
    /// <summary>
    /// Currency information
    /// </summary>
    public class Currency
    {
        public string CurrencyName { get; private set; }

        //List of currency used from the currency name
        public string[] CurrencyInfo { get; private set; }

        public Currency()
        {
            CurrencyName = "Unknown";
            CurrencyInfo = ["Unknown"];
        }

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

            //Converts the list of currency information to an array
            CurrencyInfo = [.. tempCurrencyInfo];

            //If the currency name is not found within the file, the name is taken from the file name
            if (string.IsNullOrEmpty(CurrencyName))
            {
                CurrencyName = Path.GetFileName(fileLocation);
                CurrencyName = CurrencyName[..CurrencyName.IndexOf('.')];
            }
        }
    }
}
