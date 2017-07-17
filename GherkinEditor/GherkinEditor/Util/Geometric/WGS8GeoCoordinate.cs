using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util.Geometric
{
    public class WGS8LonLat
    {
        public WGS8LonLat(double lon = 0, double lat = 0)
        {
            Lon = lon;
            Lat = lat;
        }

        public double Lon { get; set; }
        public double Lat { get; set; }
    }

    public static class WGS8GeoCoordinate
    {
        const double MAJOR_AXIS = 6378137.0;    // 地球の長半径[Semi-major axis](m)
        const double MINOR_AXIS = 6356752.314;  // 地球の短半径[](m)
        const double EPSILON = 1.0e-6;          // 最小値

        /// <summary>
        /// Convert geo-coordinates to plane coordinate by using first position as original position
        /// Unit: meter
        /// </summary>
        /// <param name="posList"></param>
        /// <returns></returns>
        public static List<GPoint> ToPlaneCoordinate(List<WGS8LonLat> posList)
        {
            List<GPoint> points = new List<GPoint>(posList.Count);
            if (posList.Count == 0) return points;

            points.Add(new GPoint(0, 0));
            for (int i = 1; i < posList.Count; i++)
            {
                double x = Lon2X(posList[i].Lon, posList[i].Lat, posList[0].Lon);
                double y = Lat2Y(posList[i].Lat, posList[0].Lat);
                points.Add(new GPoint(x, y));
            }

            return points;
        }

        public static double Lon2X(double lon, double lat, double startLon)
        {
            double d_lon = lon - startLon;
            double x = d_lon * (MAJOR_AXIS * Math.Cos(ToRad(lat)) / 360.0 * 2.0 * Math.PI);
            return Round(x);
        }

        public static double Lat2Y(double lat, double startLat)
        {
            double d_lat = lat - startLat;
            double y = d_lat * (MINOR_AXIS / 360.0 * 2.0 * Math.PI);
            return Round(y);
        }

        static double Round(double v) => Math.Abs(v) < EPSILON ? 0 : v;
        static double ToRad(double degree) => degree * Math.PI / 180.0;
        static double ToDegree(double radian) => radian * 180.0 / Math.PI;
    }
}
