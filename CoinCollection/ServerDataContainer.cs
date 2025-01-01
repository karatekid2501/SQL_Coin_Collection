using System.Data;

namespace CoinCollection
{
    /// <summary>
    /// Contains information about the coin
    /// </summary>
    /// <param name="name">Name of the coin</param>
    /// <param name="description">Description of the coin</param>
    /// <param name="dateOfCreation">Date of the creation of the coin</param>
    /// <param name="amountMade">Coin amount</param>
    /// <param name="currencyType">Coin currency type</param>
    /// <param name="originalValue">Original coin value</param>
    /// <param name="retailValue">Retail coin value</param>
    /// <param name="image">Image of the coin</param>
    public class ServerDataContainer(string name, string description, string dateOfCreation, string amountMade, string currencyType,
        string originalValue, string retailValue, string image)
    {
        public readonly string CoinInfo = $"('{name}', '{description}', '{amountMade}', '{currencyType}', '{originalValue}', '{retailValue}', '{image}')";

        public readonly string CoinDateAndTime = $"({dateOfCreation})";

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

        public ServerDataContainer(DataRowView drv, DataRowView? drvDate = null) : this
            (
            drv?[1]?.ToString() ?? "∅", 
            drv?[2]?.ToString() ?? "∅",
            drvDate?[0]?.ToString() ?? "∅", 
            drv?[3]?.ToString() ?? "∅", 
            drv?[4]?.ToString() ?? "∅", 
            drv?[5]?.ToString() ?? "∅", 
            drv?[6]?.ToString() ?? "∅", 
            drv?[7]?.ToString() ?? "∅"
            )
        {
            /*if(drv == null)
            {
                throw new NullReferenceException("DateRowView is null");
            }*/

            NotNull = drv != null;
        }

        public string this [int key]
        {
            get
            {
                return _info [key];
            }
        }
    }
}