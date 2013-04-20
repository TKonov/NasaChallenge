﻿using System;
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
using ISSLocator.Controls;
using System.Device.Location;
using ISSLocator.Data;
using System.Xml.Serialization;
using Microsoft.Phone.Shell;
using ISSLocator.LocationService;
using System.Windows.Controls.Primitives;

namespace ISSLocator
{
    public partial class MainPage : PhoneApplicationPage
    {
        private StarViewModel Model { get; set; }

        // Constructor
        public MainPage()
        {
            this.Loaded += MainPage_Loaded;

            InitializeComponent();

            this.arPanel.Unloaded += arPanel_Unloaded;

        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var model = ((App)App.Current).Model as StarViewModel;
            if (model != null)
            {
                this.Model = model;
            }
            else
            {
                this.Model = new StarViewModel();
                ((App)App.Current).Model = this.Model;
            }

            this.InitializeScene();

            var panel = this.arPanel;
            //For motion to be supported, device needs at least a compass and
            //an accelerometer. A gyro as well makes the experience much better though
            if (Microsoft.Devices.Sensors.Motion.IsSupported)
            {
                //Warn user that without gyro, the experience isn't as good as it can get
                if (!Microsoft.Devices.Sensors.Gyroscope.IsSupported)
                {
                    LayoutRoot.Children.Add(new TextBlock()
                    {
                        Text = "No gyro detected. Experience may be degraded",
                        TextWrapping = System.Windows.TextWrapping.Wrap,
                        VerticalAlignment = System.Windows.VerticalAlignment.Bottom
                    });
                }
                //Start the AR PAnel
                panel.Start();
            }
            else //Bummer! 
            {
                panel.Visibility = System.Windows.Visibility.Collapsed;
                MessageBox.Show("Sorry - Motion sensor is not supported on this device");
            }
        }

        private void AddHeadingDots(ARPanel panel)
        {
            var textForeground = new SolidColorBrush(Color.FromArgb(255, 96, 96, 96));

            //Go 360 along the horizon to display heading text
            for (double azimuth = 20; azimuth < 360; azimuth += 20)
            {
                if (azimuth % 90 == 0) continue; //skip cardinal directions, since we already added markers for those in xaml
                TextBlock tb = new TextBlock() { Text = string.Format("{0}°", azimuth), Foreground = textForeground };
                ARPanel.SetDirection(tb, new Point(0, azimuth));
                panel.Children.Add(tb);
            }
            //Display an up/down angle for each cardinal direction
            for (int i = 0; i < 360; i += 180)
            {
                for (int alt = -80; alt < 90; alt += 10)
                {
                    if (alt == 0) continue; //skip cardinal directions, since we already added markers for those in xaml
                    TextBlock tb = new TextBlock() { Text = string.Format("{0}", alt), Foreground = textForeground };
                    ARPanel.SetDirection(tb, new Point(alt, i));
                    panel.Children.Add(tb);
                }
            }
        }

        private void PositionStars(ARPanel arPanel, GeoCoordinate coordinates)
        {
            foreach (var star in Model.Data.Data)
            {


                var point = StarUtils.CalculatePosition(coordinates, star);

                if (point.X > 0)
                {
                    var starBightness = 6 - star.Mag;
                    Color color = StarUtils.GetStarColor(star.Color);
                    var ellipse = new Ellipse { Fill = new SolidColorBrush(color), Width = starBightness, Height = starBightness, Stroke = new SolidColorBrush(color) { Opacity = 0.5 }, StrokeThickness = 1 };

                    arPanel.Children.Add(ellipse);
                    ARPanel.SetDirection(ellipse, point);
                }
            }
        }

        private void PositionStation(ARPanel arPanel)
        {
            this.Model.Positions = new List<StationStat>(this.Model.Positions.Where(c => c.End.Time > DateTime.Now));

            var forecast = this.Model.Positions[0];
            var startPosition = forecast.Start;
            var topPosition = forecast.Top;
            var endPosition = forecast.End;

            AddMarker(forecast, startPosition, Color.FromArgb(100, 0, 200, 0));
            AddMarker(forecast, topPosition, Color.FromArgb(100, 255, 255, 0));
            AddMarker(forecast, endPosition, Color.FromArgb(100, 200, 0, 0));
        }

        private void AddMarker(StationStat forecast, ISSPosition startPosition, Color color)
        {
            SpotData spotData;
            Rectangle rectangle;
            Point point;

            spotData = this.GetSpotData(forecast.Start, forecast.Brightness);

            rectangle = new Rectangle { Width = 90, Height = 90, RadiusX = 90, RadiusY = 60, Fill = new SolidColorBrush(color) };
            var grid = new Grid();
            arPanel.Children.Add(rectangle);


            point = new Point(startPosition.Altitute, startPosition.Azimuth);
            ARPanel.SetDirection(rectangle, point);
            rectangle.DataContext = spotData;
            rectangle.Tap += (s, e) => { NotificationPopup.DataContext = ((FrameworkElement)s).DataContext; NotificationPopup.Open(); };
        }

        private SpotData GetSpotData(ISSPosition position, double brightness)
        {
            SpotData d = new SpotData();
            d.Altitute = position.Altitute;
            d.Azimuth = position.Azimuth;
            d.Brightness = 6 - brightness;
            d.StartTime = position.Time;
            d.TimeRemaining = DateTime.Now - position.Time;
            return d;
        }

        private GeoCoordinateWatcher watcher;

        private void InitializeScene()
        {
            AddHeadingDots(arPanel);

            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
                {
                    MovementThreshold = 500
                };

                watcher.PositionChanged += watcher_PositionChanged;
                watcher.Start();
            }
        }

        private void LoadStationForecast(GeoCoordinate coordiantes)
        {
            if (this.Model.Positions == null)
            {
                LocationService.LocationService.GetStationStats(25544, coordiantes.Latitude, coordiantes.Longitude, (s) => PersistStationData(s));
            }
            else
            {
                PositionStation(this.arPanel);
            }
        }

        private void PersistStationData(List<StationStat> stats)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    this.Model.Positions = stats;

                    PositionStation(this.arPanel);
                });
        }

        void arPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            (sender as ARPanel).Stop();
        }

        void arPanel_Loaded(object sender, RoutedEventArgs e)
        {



        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var epl = e.Position.Location;

            this.arPanel.Children.Clear();

            InitializeScene();
            PositionStars(arPanel, epl);

            this.LoadStationForecast(epl);
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/PassesList.xaml"), UriKind.Relative));



        }




    }
}