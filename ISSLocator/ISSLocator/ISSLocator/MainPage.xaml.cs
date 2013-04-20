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
using ISSLocator.Controls;
using System.Device.Location;
using ISSLocator.Data;
using System.Xml.Serialization;
using Microsoft.Phone.Shell;

namespace ISSLocator
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.InitializeScene();
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
            XmlSerializer ser = new XmlSerializer(typeof(StarData));
            StarData data;


            using (System.IO.Stream fs = Application.GetResourceStream(new Uri(@"/ISSLocator;component/starOutput.xml", UriKind.Relative)).Stream)
            {
                data = (StarData)ser.Deserialize(fs);

            }

            foreach (var star in data.Data)
            {
                var starBightness = 6 - star.Mag;
                Color color = StarUtils.GetStarColor(star.Color);
                var ellipse = new Ellipse { Fill = new SolidColorBrush(color), Width = starBightness, Height = starBightness, Stroke = new SolidColorBrush(color) { Opacity = 0.5 }, StrokeThickness = 1 };

                arPanel.Children.Add(ellipse);

                var point = StarUtils.CalculatePosition(coordinates, star);


                ARPanel.SetDirection(ellipse, point);
            }
        }

        private void InitializeScene()
        {
            AddHeadingDots(arPanel);



            var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
            {
                MovementThreshold = 20
            };

            watcher.PositionChanged += watcher_PositionChanged;
            //  watcher.StatusChanged += this.watcher_StatusChanged;
            watcher.Start();

            this.arPanel.Loaded += arPanel_Loaded;
            this.arPanel.Unloaded += arPanel_Unloaded;
        }

        void arPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            (sender as ARPanel).Stop();
        }

        void arPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as ARPanel;
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

        //private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        //{
        //    switch (e.Status)
        //    {
        //        case GeoPositionStatus.Disabled:
        //            // location is unsupported on this device
        //            break;
        //        case GeoPositionStatus.NoData:
        //            // data unavailable
        //            break;
        //    }
        //}

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var epl = e.Position.Location;


            PositionStars(arPanel, epl);
        }
    }
}