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
using System.Xml.Serialization;
using Stars.Data;
using System.IO;
using System.Device.Location;
using System.Diagnostics;
using Microsoft.Phone.Shell;
using Microsoft.Devices;

namespace ARSampleApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private VideoBrush brush;
        private PhotoCamera cam;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //Add some more elements to the panel besides the ones already added in XAML
            AddHeadingDots(arPanel);

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            GeoCoordinateWatcher watcher;

            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
            {
                MovementThreshold = 20
            };

            watcher.PositionChanged += this.watcher_PositionChanged;
            watcher.StatusChanged += this.watcher_StatusChanged;
            watcher.Start();

            var trans = new CompositeTransform();
            trans.CenterX = 0.5;
            trans.CenterY = 0.5;
            trans.Rotation = 90;

            brush = new VideoBrush()
            {
                RelativeTransform = trans
            };

         //   LayoutRoot.Background = brush;
        }

        private void ARPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as SharpGIS.AR.Controls.ARPanel;
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


            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true)
            {
                cam = new PhotoCamera(CameraType.Primary);

                if (!displayCammera)
                {


                    brush.SetSource(cam);
                    displayCammera = true;
                }
            }
            else
            {
                MessageBox.Show("A Camera is not available on this device.");
            }
        }

        void cam_CaptureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ARPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            //Remember to stop the motion sensor when leaving
            (sender as SharpGIS.AR.Controls.ARPanel).Stop();

            if (cam != null)
            {
                cam.Dispose();
            }
        }

        private void AddHeadingDots(SharpGIS.AR.Controls.ARPanel panel)
        {
            var textForeground = new SolidColorBrush(Color.FromArgb(255, 96, 96, 96));

            //Go 360 along the horizon to display heading text
            for (double azimuth = 20; azimuth < 360; azimuth += 20)
            {
                if (azimuth % 90 == 0) continue; //skip cardinal directions, since we already added markers for those in xaml
                TextBlock tb = new TextBlock() { Text = string.Format("{0}°", azimuth), Foreground = textForeground };
                SharpGIS.AR.Controls.ARPanel.SetDirection(tb, new Point(0, azimuth));
                panel.Children.Add(tb);
            }
            //Display an up/down angle for each cardinal direction
            for (int i = 0; i < 360; i += 180)
            {
                for (int alt = -80; alt < 90; alt += 10)
                {
                    if (alt == 0) continue; //skip cardinal directions, since we already added markers for those in xaml
                    TextBlock tb = new TextBlock() { Text = string.Format("{0}", alt), Foreground = textForeground };
                    SharpGIS.AR.Controls.ARPanel.SetDirection(tb, new Point(alt, i));
                    panel.Children.Add(tb);
                }
            }
        }

        private void PositionStars(SharpGIS.AR.Controls.ARPanel arPanel, GeoCoordinate coordinates)
        {
            XmlSerializer ser = new XmlSerializer(typeof(StarData));
            StarData data;


            using (System.IO.Stream fs = Application.GetResourceStream(new Uri(@"/ARSampleApp;component/starOutput.xml", UriKind.Relative)).Stream)
            {
                data = (StarData)ser.Deserialize(fs);

            }

            foreach (var star in data.Data)
            {
                var starBightness = 6 - star.Mag;
                Color color = GetStarColor(star.Color);
                var ellipse = new Ellipse { Fill = new SolidColorBrush(color), Width = starBightness, Height = starBightness, Stroke = new SolidColorBrush(color) { Opacity = 0.5 }, StrokeThickness = 1 };

                arPanel.Children.Add(ellipse);

                var point = CalculatePosition(coordinates, star);


                SharpGIS.AR.Controls.ARPanel.SetDirection(ellipse, point);
            }
        }

        private Point CalculatePosition(GeoCoordinate coordinates, Star star)
        {

            double alt = 0;
            double azim = 0;
            var ha = CoordinatesHelper.ConvRAToHA(star.RaDec, DateTime.Now.ToUniversalTime(), coordinates.Longitude);

            CoordinatesHelper.ConvEquToHor(coordinates.Longitude, ha, star.Dec, ref alt, ref  azim);

            return new Point(alt, azim);
        }

        //private static Point CalculatePosition(GeoCoordinate coordinates, Star star)
        //{
        //    var LMST = 304.80762;//coordinates.Longitude;
        //    var latRad = coordinates.Latitude / 180 * Math.PI;

        //    var ha = LMST - star.RaDec;

        //    var radDec = star.Dec / 180 * Math.PI;

        //    var sinDec = Math.Sin(radDec);
        //    var sinLat = Math.Sin(latRad);
        //    var cosLat = Math.Cos(latRad);
        //    var sinHa = Math.Sin(ha / 180 * Math.PI);

        //    var sinAlt = sinDec * sinLat + Math.Cos(radDec) * cosLat * Math.Cos(ha / 180 * Math.PI);
        //    var ALT = Math.Asin(sinAlt);

        //    var a = Math.Acos((sinDec - sinAlt * sinLat) / (Math.Cos(ALT) * cosLat));

        //    var az = sinHa < 0 ? (a * 180 / Math.PI) : (360 - a * 180 / Math.PI);
        //    var altRes = ALT * 180 / Math.PI;

        //    //var ha = LMST - star.RaDec;

        //    //var radDec = star.Dec / 180 * Math.PI;

        //    //var sinDec = Math.Sin(radDec);
        //    //var sinLat = Math.Sin(latRad);
        //    //var cosLat = Math.Cos(latRad);

        //    //var sinAlt = sinDec * sinLat + Math.Cos(radDec) * cosLat * Math.Cos(ha / 180 * Math.PI);
        //    //var ALT = Math.Asin(sinAlt);

        //    //var a = Math.Acos((sinDec - sinAlt * sinLat) / (Math.Cos(ALT) * cosLat));

        //    //var az = a >= 0 ? a * 180 / Math.PI :360 - a * 180 / Math.PI;

        //    var point = new Point(altRes, az);
        //    return point;
        //}

        private Color GetStarColor(char colorCode)
        {
            switch (colorCode)
            {
                case 'O':
                    return Color.FromArgb(255, 155, 176, 255);
                case 'B':
                    return Color.FromArgb(255, 170, 191, 255);
                case 'A':
                    return Color.FromArgb(255, 202, 216, 255);
                case 'F':
                    return Color.FromArgb(255, 155, 176, 255);
                case 'G':
                    return Color.FromArgb(255, 251, 248, 255);
                case 'K':
                    return Color.FromArgb(255, 255, 221, 180);
                case 'M':
                    return Color.FromArgb(255, 255, 189, 111);
                case 'L':
                    return Color.FromArgb(255, 248, 66, 53);
                case 'T':
                    return Color.FromArgb(255, 186, 48, 89);
                default:
                    return Colors.White;
            }
        }



        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // location is unsupported on this device
                    break;
                case GeoPositionStatus.NoData:
                    // data unavailable
                    break;
            }
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var epl = e.Position.Location;


            PositionStars(arPanel, epl);

            // Access the position information thusly:
            //epl.Latitude.ToString("0.000");
            //epl.Longitude.ToString("0.000");
            //epl.Altitude.ToString();
            //epl.HorizontalAccuracy.ToString();
            //epl.VerticalAccuracy.ToString();
            //epl.Course.ToString();
            //epl.Speed.ToString();
            //e.Position.Timestamp.LocalDateTime.ToString();
        }

        private void ApplicationBar_StateChanged_1(object sender, ApplicationBarStateChangedEventArgs e)
        {
            displayCammera = !displayCammera;

            if (!displayCammera)
            {
                brush.Opacity = 0;
            }
            else
            {
                brush.Opacity = 1;
            }
        }

        private bool displayCammera;

        private void ToggleCameraOutput_Click_1(object sender, EventArgs e)
        {

        }
    }
}