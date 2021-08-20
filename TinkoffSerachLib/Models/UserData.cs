using System;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffSerachLib.Models
{
    [Serializable]
    public class UserData
    {
        public string Token { get; set; }
        public decimal MoneyLimitValue { get; set; }
        public Currency Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Linearity { get; set; }
        
    }
}
