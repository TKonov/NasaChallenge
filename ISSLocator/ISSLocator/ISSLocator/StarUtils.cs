using ISSLocator.Data;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ISSLocator
{
    public static class StarUtils
    {
        public static Point CalculatePosition(GeoCoordinate coordinates, Star star)
        {

            double alt = 0;
            double azim = 0;
            var ha = CoordinatesHelper.ConvRAToHA(star.RaDec, DateTime.Now.ToUniversalTime(), coordinates.Longitude);

            CoordinatesHelper.ConvEquToHor(coordinates.Longitude, ha, star.Dec, ref alt, ref  azim);

            return new Point(alt, azim);

        }

        public static Color GetStarColor(char colorCode)
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
    }
}
