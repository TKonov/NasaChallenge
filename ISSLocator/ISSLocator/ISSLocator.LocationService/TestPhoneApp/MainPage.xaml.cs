using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ISSLocator.LocationService;

namespace TestPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //int a = int.Parse("a");

            //LocationService.GetStationStats(25544, 42.693539, 23.302002, OnStatsReturned);
        }

        private void OnStatsReturned(string result)
        {
            Dispatcher.BeginInvoke(() =>
            {
                this.ContentBox.Text = result;
            });
        }
    }
}