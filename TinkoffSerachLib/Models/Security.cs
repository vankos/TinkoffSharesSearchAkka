using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffSearchLib.Models
{
    public class Security
    {
        public string Name { get; set; }
        public decimal Growth { get; set; }
        public decimal Linearity { get; set; }

        public List<CandlePayload> Candles { get; set;}

    }
}
