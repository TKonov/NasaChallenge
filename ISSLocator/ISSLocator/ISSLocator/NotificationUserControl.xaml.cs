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
                Alarm alarm = new Alarm(string.Format("Nasa ISS pass with {0} brightness.", (item as StationStat).Brightness));
                alarm.BeginTime = stationStart.AddMinutes(-sliderValue);
                alarm.ExpirationTime = alarm.BeginTime.AddMinutes(5);

                //ScheduledActionService.Add(alarm);
                var action = ScheduledActionService.GetActions<Alarm>().FirstOrDefault(x=>x.Name == alarm.Name);
                if (action == null)
                {
                    ScheduledActionService.Add(alarm);
                    MessageBox.Show(string.Format("Alarm set for {0}\nExpiration time: {1}\n", alarm.BeginTime, alarm.ExpirationTime));
                }
            }

        }

        private bool valueChanging;

        private void alarmSlider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (!valueChanging && slider != null)
            {
                valueChanging = true;
                slider.Value = Math.Round(e.NewValue * 10) / 10;
                valueChanging = false;
            }
        }
    }
}
