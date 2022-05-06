using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Models
{
    public class AggregatedData
    {
        public DateTime Time { get; set; }
        public string Link { get; set; }
        public string NeType { get; set; }
        public string NeAlias { get; set; }
        public double MAX_RX_LEVEL { get; set; }
        public double MAX_TX_LEVEL { get; set; }
        public double RSL_DEVIATION { get; set; }
    }
}
