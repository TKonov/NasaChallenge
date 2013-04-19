using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Stars.Data
{
    public class Star
    {
        [XmlElement(ElementName = "RARaw")]
        public DateTime RaRaw { get; set; }

        [XmlElement(ElementName = "RADeg")]
        public double RaDec { get; set; }

        [XmlElement(ElementName = "Dec")]
        public double Dec { get; set; }

        [XmlElement(ElementName = "Mag")]
        public double Mag { get; set; }

        [XmlElement(ElementName = "Color")]
        public char Color { get; set; }

        [XmlElement(ElementName = "TitleId")]
        public int TitleId { get; set; }





    }
}
