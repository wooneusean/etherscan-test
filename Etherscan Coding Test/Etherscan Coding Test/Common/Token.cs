using System;

namespace Etherscan_Coding_Test
{
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