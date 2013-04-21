using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using ISSLocator.LocationService;

namespace ISSLocator
{
    public partial class NotificationUserControl : UserControl
    {
        public NotificationUserControl()
        {
            InitializeComponent();
     
        }

        private void SubsribeAlarm(object sender, RoutedEventArgs e)
        {
            var sliderValue = this.alarmSlider.Value;
            List<StationStat> stationStatCollection = ((sender as Button).DataContext as List<StationStat>);
            foreach (var item in stationStatCollection)
            {
                DateTime stationStart = (DateTime)(item as StationStat).Start.Time;
                Alarm alarm = new Alarm("Alarm");
                alarm.BeginTime = stationStart.AddMinutes(-sliderValue);
                alarm.ExpirationTime = alarm.BeginTime.AddMinutes(5);
                ScheduledActionService.Add(alarm);
            }

        }
    }
}
