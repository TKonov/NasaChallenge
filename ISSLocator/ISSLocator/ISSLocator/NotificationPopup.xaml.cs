using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;

namespace ISSLocator
{
    public partial class NotificationPopup : UserControl
    {
      //  private DispatcherTimer timer;

        public NotificationPopup()
        {
            InitializeComponent();
        //    timer = new DispatcherTimer();
        //    timer.Tick += timer_Tick;
        //    timer.Interval = new TimeSpan(0, 0, 5);
        }

        public void Open()
        {
            NotificationPanel.Visibility = Visibility.Visible;
           // timer.Start();
        }

        public void Close()
        {
            NotificationPanel.Visibility = Visibility.Collapsed;
        }

        //void timer_Tick(object sender, EventArgs e)
        //{
        //    NotificationPanel.Visibility = Visibility.Collapsed;
        //    timer.Stop();
        //}
    }
}
