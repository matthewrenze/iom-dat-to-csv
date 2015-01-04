using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IomDatToCsv
{
    public class Record
    {
        public DateTime DateTime { get; set; }

        public int TimeStamp { get; set; }

        public int ElectroDermalResponse { get; set; }

        public int Detect { get; set; }

        public decimal HeartRate { get; set; }

        public string Label { get; set; }

        public int Coherence { get; set; }
    }
}
