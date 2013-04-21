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

            if (stationStatCollection == null)
            {
                return;
            }

            foreach (var item in stationStatCollection)
            {
                DateTime stationStart = (DateTime)(item as StationStat).Start.Time;
                Alarm alarm = new Alarm(string.Format("Nasa ISS pass at {0} with {1} brightness.", (item as StationStat).Start.Time, (item as StationStat).Brightness));
                alarm.BeginTime = stationStart.AddMinutes(-sliderValue);
                alarm.ExpirationTime = alarm.BeginTime.AddMinutes(5);
                alarm.Content = alarm.Name;

                //ScheduledActionService.Add(alarm);
                var action = ScheduledActionService.GetActions<Alarm>().FirstOrDefault(x => x.Name == alarm.Name);
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
            if (!valueChanging && slider != null && alarmButton != null)
            {
                valueChanging = true;
                var value = Math.Round(e.NewValue * 2) / 2;
                if (value < 5)
                {
                    value = 0;
                    alarmButton.IsEnabled = false;
                }
                else
                {
                    value = value + (5 - value % 5);
                    alarmButton.IsEnabled = true;
                }

                slider.Value = value;
                valueChanging = false;
            }
        }
    }
}
