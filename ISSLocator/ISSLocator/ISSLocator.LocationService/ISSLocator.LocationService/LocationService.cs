using HtmlAgilityPack;
using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ISSLocator.LocationService
{
    public class LocationService
    {
        public static Uri GetStationStats(int sateliteId, double latitude, double longitude, Action<List<StationStat>> callback, string location = "unknown")
        {
            Uri uri = BuildUrl(sateliteId, latitude, longitude, location);

            if (uri != null)
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.BeginGetResponse(r =>
                {
                    var httpRequest = (HttpWebRequest)r.AsyncState;
                    var httpResponse = (HttpWebResponse)httpRequest.EndGetResponse(r);

                    using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var stats = GetStats(reader);
                        callback(stats);
                    }
                }, request);
            }

            return uri;
        }

        private static List<StationStat> GetStats(StreamReader reader)
        {
            HtmlDocument document = new HtmlDocument();
            document.Load(reader);

            List<StationStat> stats = new List<StationStat>();

            var nodes = new List<HtmlNode>();
            GetNodesRecursive(document.DocumentNode, nodes);

            foreach (var node in nodes)
            {
                try
                {
                    StationStat stat = ParseRowToStat(node);
                    stats.Add(stat);
                }
                catch (InvalidCastException e)
                {
                    Debug.WriteLine(e.Message);
                }
                catch (FormatException e)
                {
                    Debug.WriteLine(e.Message);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return stats;
        }

        private static void GetNodesRecursive(HtmlNode htmlNode, List<HtmlNode> nodes)
        {
            foreach (var node in htmlNode.ChildNodes)
            {
                GetNodesRecursive(node, nodes);
            }

            if (htmlNode.GetAttributeValue("class", "").ToLower().Contains("clickablerow"))
            {
                nodes.Add(htmlNode);
            }
        }

        private static StationStat ParseRowToStat(HtmlNode node)
        {
            StationStat stat = new StationStat();
            var statStrings = node.ChildNodes.Select(x => x.InnerText).ToList();
            string dateString = statStrings[0];
            double magnitude = double.Parse(statStrings[1]);

            string startTime = statStrings[2];
            string startAlt = statStrings[3];
            string startAzimuth = statStrings[4];

            string topTime = statStrings[5];
            string topAlt = statStrings[6];
            string topAzimuth = statStrings[7];

            string endTime = statStrings[8];
            string endAlt = statStrings[9];
            string endAzimuth = statStrings[10];

            string passType = statStrings[11];

            stat.Brightness = magnitude;
            stat.PassType = passType;
            stat.Start = new ISSPosition()
            {
                Time = TimeFromString(startTime, dateString),
                Altitute = AltitudeFromString(startAlt),
                Azimuth = AzimuthFromString(startAzimuth)
            };

            stat.Top = new ISSPosition()
            {
                Time = TimeFromString(topTime, dateString),
                Altitute = AltitudeFromString(topAlt),
                Azimuth = AzimuthFromString(topAzimuth)
            };

            stat.End = new ISSPosition()
            {
                Time = TimeFromString(endTime, dateString),
                Altitute = AltitudeFromString(endAlt),
                Azimuth = AzimuthFromString(endAzimuth)
            };

            return stat;
        }

        private static double AzimuthFromString(string startAzimuth)
        {
            switch (startAzimuth.ToLower())
            {
                case "n": return 0;
                case "nne": return 22.5;
                case "ne": return 45;
                case "ene": return 67.5;
                case "e": return 90;
                case "ese": return 112.5;
                case "se": return 135;
                case "sse": return 157.5;
                case "s": return 180;
                case "ssw": return 202.5;
                case "sw": return 225;
                case "wsw": return 247.5;
                case "w": return 270;
                case "wnw": return 292.5;
                case "nw": return 315;
                case "nnw": return 337.5;
                default: return 0;
            }
        }

        private static double AltitudeFromString(string altString)
        {
            var altStr = altString.Remove(altString.IndexOf('°'), 1);

            return double.Parse(altStr);
        }

        private static DateTime TimeFromString(string timeString, string dateString)
        {
            string[] date = dateString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int dateNum = int.Parse(date[0]);
            string month = date[1];

            var timeArr = timeString.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToList();

            DateTime time = new DateTime(DateTime.Now.Year, MonthFromString(month), dateNum,
                timeArr[0], timeArr[1], timeArr[2]);

            return time;        
        }

        private static int MonthFromString(string month)
        {
            switch (month.ToLower())
            {
                case "jan":
                    {
                        return 1;
                    }
                    break;
                case "feb":
                    {
                        return 2;
                    }
                    break;
                case "mar":
                    {
                        return 3;
                    }
                    break;
                case "apr":
                    {
                        return 4;
                    }
                    break;
                case "may":
                    {
                        return 5;
                    }
                    break;
                case "jun":
                    {
                        return 6;
                    }
                    break;
                case "jul":
                    {
                        return 7;
                    }
                    break;
                case "aug":
                    {
                        return 8;
                    }
                    break;
                case "sep":
                    {
                        return 9;
                    }
                    break;
                case "oct":
                    {
                        return 10;
                    }
                    break;
                case "nov":
                    {
                        return 11;
                    }
                    break;
                case "dec":
                    {
                        return 12;
                    }
                    break;
                default:
                    return -1;
                    break;
            }
        }
  
        private static Uri BuildUrl(int sateliteId, double latitude, double longitude, string location)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("http://www.heavens-above.com/PassSummary.aspx?satid=");
            sb.Append(sateliteId);
            sb.Append("&lat=");
            sb.Append(latitude);
            sb.Append("&lng=");
            sb.Append(longitude);
            sb.Append("&loc=");
            sb.Append(location);
            sb.Append("&alt=0&tz=UCT");

            Uri uri = null;

            Debug.WriteLine(sb.ToString());

            Uri.TryCreate(sb.ToString(), UriKind.Absolute, out uri);

            return uri;
        }
    }
}