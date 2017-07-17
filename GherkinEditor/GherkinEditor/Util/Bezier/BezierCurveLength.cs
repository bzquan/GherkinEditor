using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    public static class BezierCurveLength
    {
        public static double LengthByQuadratic(List<GPoint> points)
        {
            if (points.Count < 2) return 0.0;

            int degree = 2;
            List<GPoint> paddedPoints = PadPoints(points, degree);
            double length = 0.0;
            for (int i = 0; i < paddedPoints.Count - degree; i += degree)
            {
                length += BezierCurveLength2DQuadratic.Length(paddedPoints[i], paddedPoints[i + 1], paddedPoints[i + 2]);
            }

            return length;
        }

        public static double LengthByQuadratic3D(List<GPoint> points)
        {
            if (points.Count < 2) return 0.0;

            int degree = 2;
            List<GPoint> paddedPoints = PadPoints(points, degree);
            double length = 0.0;
            for (int i = 0; i < paddedPoints.Count - degree; i += degree)
            {
                var a = new V3D(paddedPoints[i].X, paddedPoints[i].Y);
                var b = new V3D(paddedPoints[i + 1].X, paddedPoints[i + 1].Y);
                var c = new V3D(paddedPoints[i + 2].X, paddedPoints[i + 2].Y);
                BezierCurveLength3DQuadratic lenCalc = new BezierCurveLength3DQuadratic(a, b, c);
                length += lenCalc.Length;
            }

            return length;
        }

        public static double LengthByCubic3D(List<GPoint> points)
        {
            if (points.Count < 2) return 0.0;

            int degree = 3;
            List<GPoint> paddedPoints = PadPoints(points, degree);
            double length = 0.0;
            for (int i = 0; i < paddedPoints.Count - degree; i += degree)
            {
                var a = new V3D(paddedPoints[i].X, paddedPoints[i].Y);
                var b = new V3D(paddedPoints[i + 1].X, paddedPoints[i + 1].Y);
                var c = new V3D(paddedPoints[i + 2].X, paddedPoints[i + 2].Y);
                var d = new V3D(paddedPoints[i + 3].X, paddedPoints[i + 3].Y);
                BezierCurveLength3DCubic lenCalc = new BezierCurveLength3DCubic(a, b, c, d);
                length += lenCalc.Length;
            }
            
            return length;
        }

        private static List<GPoint> PadPoints(List<GPoint> points, int degree)
        {
            int order = degree + 1;
            int padCount = (order - points.Count % order) % order;
            if (padCount == 0) return points;

            List<GPoint> paddedPoints = new List<GPoint>(points);

            GPoint lastPoint = points.Last();
            for (int i = 0; i < padCount; i++)
            {
                paddedPoints.Add(lastPoint);
            }
            return paddedPoints;
        }
    }
}
