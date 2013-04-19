using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlServerCe;
using Stars.Data;
using System.Xml.Serialization;

namespace ConsoleApplication2
{
    class Program
    {
        private static StarData starData = new StarData();

        static void Main(string[] args)
        {
            //Load();

            using (StreamReader reader = new StreamReader("../../starCatalogue.txt"))
            {
                reader.ReadLine();
                reader.ReadLine();
                var line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    ParceStar(line);
                    line = reader.ReadLine();
                }
            }

            OrderByBrigthness();

            Save();

        }

        private static void OrderByBrigthness()
        {
            starData.Data = starData.Data.OrderBy(c => c.Mag).Take(1000).ToList();
        }

        private static void ParceStar(string line)
        {
            var data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var star = new Star();

            star.RaRaw = DateTime.ParseExact(data[1], "HH:mm:ss.ff", new CultureInfo("en-US"));
            star.RaDec = star.RaRaw.Hour * 15 + star.RaRaw.Minute / 4.0 + star.RaRaw.Second / 240.0 + star.RaRaw.Millisecond * 0.007;

            if (star.RaDec > 180)
            {
                star.RaDec = 360 - star.RaDec;
            }

            var dec = GetDegreece(data[2]);
            star.Dec = dec;

            double magnitude;
            if (double.TryParse(data[6], out magnitude))
            {
                star.Mag = magnitude;
            }
            else
            {
                return;
            }

            star.Color = data[9].Where(c => char.IsUpper(c) && char.IsLetter(c)).FirstOrDefault();

            star.TitleId = int.Parse(data[7]);
            starData.Data.Add(star);
        }

        private static double GetDegreece(string p)
        {
            var parts = p.Split(':', '.');

            var deg = double.Parse(parts[0]);
            var min = double.Parse(parts[1]) / 60.0;
            var sec = double.Parse(parts[2]) / 3600.0;

            var result = (Math.Abs(deg) + min + sec);

            return Math.Sign(deg) < 0 ? -result : result;
        }

        private static void Load()
        {
            XmlSerializer ser = new XmlSerializer(typeof(StarData));
            StarData data;


            using (FileStream fs = new FileStream("starOutput.xml", FileMode.Open))
            {
                data = (StarData)ser.Deserialize(fs);
            }

            Console.WriteLine("Data loaded");
        }

        private static void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(StarData));

            using (FileStream fs = new FileStream("starOutput.xml", FileMode.OpenOrCreate))
            {
                ser.Serialize(fs, starData);
            }
            //string strConnection = "StarData.sdf";
            //using (var conn = new SqlCeConnection(string.Format("Data Source={0};Default Lock Escalation =100;", strConnection)))
            //{
            //    conn.Open();

            //    try
            //    {
            //        //your Stuff                    
            //    }
            //    catch (SqlCeException)
            //    {
            //        throw;
            //    }
            //    finally
            //    {
            //        if (conn.State == ConnectionState.Open) conn.Close();
            //    }
            //}
        }
    }
}
