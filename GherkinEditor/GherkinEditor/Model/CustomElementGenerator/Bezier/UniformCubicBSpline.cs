using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    /// <summary>
    /// Uniform Cubic B-Spline Curve
    /// http://www2.cs.uregina.ca/~anima/408/Notes/Interpolation/UniformBSpline.htm
    /// </summary>
    public class UniformCubicBSpline : IBSpline
    {
        private int m_TotalPoints;
        private List<GPoint> m_Points;

        public UniformCubicBSpline(List<GPoint> points)
        {
            m_Points = points;
            m_TotalPoints = points.Count;
        }

        public GPoint GetPoint(double t)
        {
            if (!HasBSplineSegments(t)) return null;

            int index = Index(t);
            double u = (t * m_TotalPoints - index);

            int i = index - 1;
            double x = B0(u) * m_Points[i].X +
                       B1(u) * m_Points[i + 1].X +
                       B2(u) * m_Points[i + 2].X +
                       B3(u) * m_Points[i + 3].X;

            double y = B0(u) * m_Points[i].Y +
                       B1(u) * m_Points[i + 1].Y +
                       B2(u) * m_Points[i + 2].Y +
                       B3(u) * m_Points[i + 3].Y;

            return new GPoint(x, y);
        }

        public double? Curvature(double t)
        {
            if (!HasBSplineSegments(t)) return null;

            int index = Index(t);
            double u = (t * m_TotalPoints - index);

            int i = index - 1;
            double x1 = B0d1(u) * m_Points[i].X +
                        B1d1(u) * m_Points[i + 1].X +
                        B2d1(u) * m_Points[i + 2].X +
                        B3d1(u) * m_Points[i + 3].X;

            double y1 = B0d1(u) * m_Points[i].Y +
                        B1d1(u) * m_Points[i + 1].Y +
                        B2d1(u) * m_Points[i + 2].Y +
                        B3d1(u) * m_Points[i + 3].Y;

            double x2 = B0d2(u) * m_Points[i].X +
                        B1d2(u) * m_Points[i + 1].X +
                        B2d2(u) * m_Points[i + 2].X +
                        B3d2(u) * m_Points[i + 3].X;

            double y2 = B0d2(u) * m_Points[i].Y +
                        B1d2(u) * m_Points[i + 1].Y +
                        B2d2(u) * m_Points[i + 2].Y +
                        B3d2(u) * m_Points[i + 3].Y;

            double curvature = (y1 * x2 - x1 * y2) / Math.Pow(x1 * x1 + y1 * y1, 1.5);
            double scale = (CurveViewCache.Instance.CurvatureUnit == CurvatureUnit.Centimeter) ? 0.01 : 1.0; // 1/cm or 1/m
            return curvature * scale;
        }

        private bool HasBSplineSegments(double t)
        {
            int index = Index(t);
            return ((index > 0) && (index <= m_TotalPoints - 3));
        }

        private int Index(double t) => (int)(t * m_TotalPoints);

        private double B0(double u)
            => (1 - u) * (1 - u) * (1 - u) / 6.0;

        private double B1(double u)
            => (3.0 * u * u * u - 6.0 * u * u + 4.0) / 6.0;

        private double B2(double u)
            => (-3.0 * u * u * u + 3.0 * u * u + 3.0 * u + 1.0) / 6.0;

        private double B3(double u)
            => (u * u * u) / 6.0;

        // B0' = (-u^2 + 2.0 * u - 1) / 2.0
        private double B0d1(double u)
            => (-u * u  + 2.0 * u - 1) / 2.0;

        // B1' = 1.5 * u^2 - 2 * u
        private double B1d1(double u)
            => 1.5 * u * u - 2.0 * u;

        // B2' = -1.5 * u^2 + u + 0.5
        private double B2d1(double u)
            => -1.5 * u * u + u + 0.5;

        // B3' = 0.3 * u^2
        private double B3d1(double u)
            => 0.5 * u * u;

        // B0" = 1 - u
        private double B0d2(double u)
            => 1 - u;

        // B1" = 3.0 * u - 2
        private double B1d2(double u)
            => 3.0 * u - 2.0;

        // B2" = -3.0 * u + 1
        private double B2d2(double u)
            => -3.0 * u + 1.0;

        // B3" = u
        private double B3d2(double u)
            => u;
    }
}
