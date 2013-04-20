using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISSLocator
{
    public class SpotData
    {
        public double Azimuth { get; set; }
        public double Altitute { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public DateTime StartTime { get; set; }
        public double Brightness { get; set; }
    }
}
