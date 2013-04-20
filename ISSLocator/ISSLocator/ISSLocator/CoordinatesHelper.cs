using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISSLocator.Data
{
   public  class CoordinatesHelper
    {
        public static double ConvRAToHA(double fRA, DateTime dUT, double fLong)
        {
            //Convert Right Ascension to Hour Angle at specified time and longitude
            double fLST;
            double fHA;

            fLST = ConvTimeToDec(CalcLSTFromUT(dUT, fLong));

            fHA = fLST - fRA;
            fHA = Trig.PutIn24Hour(fHA);

            return fHA;
        }

        public static double ConvHAToRA(double fHA, DateTime dUT, double fLong)
        {
            //Convert Hour Angle to Right Ascension at specified time and longitude
            double fLST;
            double fRA;

            fLST = ConvTimeToDec(CalcLSTFromUT(dUT, fLong));

            fRA = fLST - fHA;
            fRA = Trig.PutIn24Hour(fRA);

            return fRA;
        }

        public static void ConvEquToHor(double fLatitude, double fHA, double fDecl, ref double fAlt, ref double fAzim)
        {
            double fSinAlt;
            double fCosAzim;

            fHA = Trig.DegToRad(fHA * 15);
            fDecl = Trig.DegToRad(fDecl);
            fLatitude = Trig.DegToRad(fLatitude);
            fSinAlt = (Math.Sin(fDecl) * Math.Sin(fLatitude)) + (Math.Cos(fDecl) * Math.Cos(fLatitude) * Math.Cos(fHA));
            fAlt = Math.Asin(fSinAlt);
            fCosAzim = ((Math.Sin(fDecl) - (Math.Sin(fLatitude) * Math.Sin(fAlt))) / (Math.Cos(fLatitude) * Math.Cos(fAlt)));
            fAzim = Trig.RadToDeg(Math.Acos(fCosAzim));
            if (Math.Sin(fHA) > 0)
            {
                fAzim = 360 - fAzim;
            }
            fAlt = Trig.RadToDeg(fAlt);
        }

        public static void ConvHorToEqu(double fLatitude, double fAlt, double fAzim, ref double fHA, ref double fDecl)
        {
            double fSinDecl;
            double fCosH;

            fAlt = Trig.DegToRad(fAlt);
            fAzim = Trig.DegToRad(fAzim);
            fLatitude = Trig.DegToRad(fLatitude);
            fSinDecl = (Math.Sin(fAlt) * Math.Sin(fLatitude)) + (Math.Cos(fAlt) * Math.Cos(fLatitude) * Math.Cos(fAzim));
            fDecl = Math.Asin(fSinDecl);
            fCosH = ((Math.Sin(fAlt) - (Math.Sin(fLatitude) * Math.Sin(fDecl))) / (Math.Cos(fLatitude) * Math.Cos(fDecl)));
            fHA = Trig.RadToDeg(Math.Acos(fCosH));
            if (Math.Sin(fAzim) > 0)
            {
                fHA = 360 - fHA;
            }

            fDecl = Trig.RadToDeg(fDecl);
            fHA = fHA / 15.0;
        }

        public static double ConvTimeToDec(DateTime dDate)
        {
            double fHour;

            fHour = dDate.Hour + (dDate.Minute / 60.0) + (dDate.Second / 60.0 / 60.0) + (dDate.Millisecond / 60.0 / 60.0 / 1000.0);
            return fHour;
        }

        public static DateTime CalcLSTFromUT(DateTime dDate, double fLong)
        {
            double fGST;
            double fLST;
            DateTime dLST;
            bool bAdd = false;
            double fTimeDiff;

            fGST = ConvTimeToDec(CalcGSTFromUT(dDate));
            fTimeDiff = ConvTimeToDec(ConvLongTUraniaTime(fLong, ref bAdd));

            if (bAdd == true)
            {
                fLST = fGST + fTimeDiff;
            }
            else
            {
                fLST = fGST - fTimeDiff;
            }

            while (fLST > 24)
            {
                fLST = fLST - 24;
            }

            while (fLST < 0)
            {
                fLST = fLST + 24;
            }
            dLST = ConvDecTUraniaTime(fLST);
            return dLST;
        }

        public static DateTime ConvLongTUraniaTime(double fLong, ref bool bAdd)
        {
            //double fHours;
            double fMinutes;
            //double fSeconds;
            DateTime dDate;
            //DateTime dTmpDate;

            fMinutes = fLong * 4;
            if (fMinutes < 0)
            {
                bAdd = false;
            }
            else
            {
                bAdd = true;
            }
            fMinutes = Math.Abs(fMinutes);

            dDate = new DateTime();
            dDate = dDate.AddMinutes(fMinutes);
            return dDate;
        }

        public static DateTime CalcGSTFromUT(DateTime dDate)
        {
            double fJD;
            double fS;
            double fT;
            double fT0;
            DateTime dGST;
            double fUT;
            double fGST;

            fJD = GetJulianDay(dDate.Date, 0);
            fS = fJD - 2451545.0;
            fT = fS / 36525.0;
            fT0 = 6.697374558 + (2400.051336 * fT) + (0.000025862 * fT * fT);
            fT0 = Trig.PutIn24Hour(fT0);
            fUT = ConvTimeToDec(dDate);
            fUT = fUT * 1.002737909;
            fGST = fUT + fT0;
            while (fGST > 24)
            {
                fGST = fGST - 24;
            }
            dGST = ConvDecTUraniaTime(fGST);
            return dGST;
        }

        public static DateTime ConvDecTUraniaTime(double fTime)
        {
            DateTime dDate;

            dDate = new DateTime();
            dDate = dDate.AddHours(fTime);
            return (dDate);
        }

        public static double GetJulianDay(DateTime dDate, int iZone)
        {
            double fJD;
            double iYear;
            double iMonth;
            double iDay;
            double iHour;
            double iMinute;
            double iSecond;
            double iGreg;
            double fA;
            double fB;
            double fC;
            double fD;
            double fFrac;

            dDate = CalcUTFromZT(dDate, iZone);

            iYear = dDate.Year;
            iMonth = dDate.Month;
            iDay = dDate.Day;
            iHour = dDate.Hour;
            iMinute = dDate.Minute;
            iSecond = dDate.Second;
            fFrac = iDay + ((iHour + (iMinute / 60) + (iSecond / 60 / 60)) / 24);
            if (iYear < 1582)
            {
                iGreg = 0;
            }
            else
            {
                iGreg = 1;
            }
            if ((iMonth == 1) || (iMonth == 2))
            {
                iYear = iYear - 1;
                iMonth = iMonth + 12;
            }

            fA = (long)Math.Floor(iYear / 100);
            fB = (2 - fA + (long)Math.Floor(fA / 4)) * iGreg;
            if (iYear < 0)
            {
                fC = (int)Math.Floor((365.25 * iYear) - 0.75);
            }
            else
            {
                fC = (int)Math.Floor(365.25 * iYear);
            }
            fD = (int)Math.Floor(30.6001 * (iMonth + 1));
            fJD = fB + fC + fD + 1720994.5;
            fJD = fJD + fFrac;
            return fJD;
        }

        public static DateTime CalcUTFromZT(DateTime dDate, int iZone)
        {
            if (iZone >= 0)
            {
                return dDate.Subtract(new TimeSpan(iZone, 0, 0));
            }
            else
            {
                return dDate.AddHours(Math.Abs(iZone));
            }
        }

    }
}
