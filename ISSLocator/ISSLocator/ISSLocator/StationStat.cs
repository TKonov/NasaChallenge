using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISSLocator
{
    public class StationStat
    {
        public double  Brightness { get; set; }

        public string PassType { get; set; }

        public ISSPosition Start { get; set; }

        public ISSPosition Top { get; set; }

        public ISSPosition End { get; set; }


    }
}
