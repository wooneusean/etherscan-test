using System;

namespace Etherscan_Coding_Test
{
    /**
     * The Token model with additional `Rank` and `TotalSupplyPercentage` fields as
     * I calcualted it with SQL queries and pass it into the object for ease of use.
     */
    public class Token
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public Int64 TotalSupply { get; set; }
        public string ContractAddress { get; set; }
        public int TotalHolders { get; set; }
        public decimal Price { get; set; }
        public int Rank { get; internal set; }
        public decimal TotalSupplyPercentage { get; internal set; }
    }
}