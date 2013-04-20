using ISSLocator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace ISSLocator
{
    public class StarViewModel
    {

        public StarViewModel()
        {
            XmlSerializer ser = new XmlSerializer(typeof(StarData));


            using (System.IO.Stream fs = Application.GetResourceStream(new Uri(@"/ARSampleApp;component/starOutput.xml", UriKind.Relative)).Stream)
            {
                Data = (StarData)ser.Deserialize(fs);

            }
        }

        public StarData Data { get; private set; }
    }
}
