using ISSLocator.LocationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ISSLocator
{
    public class DurationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            StationStat stat = value as StationStat;
            var sub = stat.End.Time.Subtract(stat.Start.Time);
            var minutes = sub.Minutes;
            var seconds = sub.Seconds;
            return String.Format("{0}m {1}s", minutes, seconds);
            return sub;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
