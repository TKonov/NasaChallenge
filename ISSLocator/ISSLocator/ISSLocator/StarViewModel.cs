using ISSLocator.Data;
using ISSLocator.LocationService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace ISSLocator
{
    public class StarViewModel : INotifyPropertyChanged
    {

        public StarViewModel()
        {
            XmlSerializer ser = new XmlSerializer(typeof(StarData));


            using (System.IO.Stream fs = Application.GetResourceStream(new Uri(@"/ISSLocator;component/starOutput.xml", UriKind.Relative)).Stream)
            {
                Data = (StarData)ser.Deserialize(fs);

            }
        }

        private List<StationStat> positions;

        public List<StationStat> Positions
        {
            get { return positions; }
            set
            {
                positions = value;
                this.OnPropertyChanged("Positions");
            }
        }

        private StarData data;

        public StarData Data
        {
            get { return data; }
            set
            {
                data = value;
                this.OnPropertyChanged("Data");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
