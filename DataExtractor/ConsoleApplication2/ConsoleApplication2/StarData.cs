using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Stars.Data
{
    public class StarData
    {
        public StarData()
        {
            this.Data = new List<Star>();
        }

        [XmlElement(ElementName = "Star")]
        public List<Star> Data { get; set; }
    }
}
