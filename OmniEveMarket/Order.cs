using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveMarket
{
    public class Order
    {
        public int Region { get; set; }
        public int Station { get; set; }
        public string StationName { get; set; }
        public double Security { get; set; }
        public int Range { get; set; }
        public decimal Price { get; set; }
        public int VolRemain { get; set; }
        public int MinVolume { get; set; }
        public string Expires { get; set; }
        public string ReportedTime { get; set; }
    }
}
