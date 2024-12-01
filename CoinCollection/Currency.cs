using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinCollection
{
    public class Currency
    {
        public string CurrencyName { get; private set; }

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

            CurrencyInfo = [.. tempCurrencyInfo];

            if (string.IsNullOrEmpty(CurrencyName))
            {
                CurrencyName = Path.GetFileName(fileLocation);
                CurrencyName = CurrencyName[..CurrencyName.IndexOf('.')];
            }
        }
    }
}
