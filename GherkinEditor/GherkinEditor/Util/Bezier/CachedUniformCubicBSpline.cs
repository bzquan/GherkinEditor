﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    /// <summary>
    /// Cached uniform Cubic B-Spline Curve
    /// B-Spline segment are cached
    /// http://www2.cs.uregina.ca/~anima/408/Notes/Interpolation/UniformBSpline.htm
    /// </summary>
    public class CachedUniformCubicBSpline : IBSpline
    {
        private IBSpline[] m_BSplineSegments;
        private int m_TotalPoints;
        private static CurvatureUnit s_CurvatureUnit;

        public CachedUniformCubicBSpline(List<GPoint> points, bool useMatrix, CurvatureUnit curvatureUnit)
        {
            m_TotalPoints = points.Count;
            MakeBSplineSegments(points, useMatrix);
            s_CurvatureUnit = curvatureUnit;
        }

        public GPoint GetPoint(double t)
        {
            if (HasBSplineSegments(t))
            {
                int index = Index(t);
                double u = (t * m_TotalPoints - index);
                return m_BSplineSegments[index - 1].GetPoint(u);
            }
            else
                return null;
        }

        /// <summary>
        /// Detect noise in curvature with given threshold
        /// </summary>
        /// <param name="curvatures"></param>
        /// <param name="thresholdInCM">unit is 1/cm. Default is 0.0002(1/cm), which is 5m in radius</param>
        public static void DetectNoiseCurvatures(List<GPoint> curvatures, double thresholdInCM = 0.0002)
        {
            if (s_CurvatureUnit == CurvatureUnit.Meter)
            {
                thresholdInCM *= 100.0;
            }
            for (int i = 1; i < curvatures.Count - 1; i++)
            {
                var v0 = curvatures[i - 1].Y;
                var v1 = curvatures[i].Y;
                var v2 = curvatures[i + 1].Y;
                curvatures[i].IsNoise = ((Math.Abs(v1 - v0) > thresholdInCM) || (Math.Abs(v2 - v1) > thresholdInCM)) &&
                                        (Math.Sign(v1 - v0) != Math.Sign(v2 - v1));
            }
        }

        public double? Curvature(double t)
        {
            if (HasBSplineSegments(t))
            {
                int index = Index(t);
                double scale = (s_CurvatureUnit == CurvatureUnit.Centimeter) ? 0.01 : 1.0; // 1/cm or 1/m
                double u = (t * m_TotalPoints - index);
                return m_BSplineSegments[index - 1].Curvature(u) * scale;
            }
            else
                return null;
        }

        private bool HasBSplineSegments(double t)
        {
            int index = Index(t);
            int length = m_BSplineSegments.Length;
            return ((index > 0) && (index <= length));
        }

        private int Index(double t) => (int)(t * m_TotalPoints);

        private void MakeBSplineSegments(List<GPoint> points, bool useMatrix)
        {
            m_BSplineSegments = new IBSpline[points.Count - 3];
            for (int i = 1; i < points.Count - 2; i++)
            {
                if (useMatrix)
                    m_BSplineSegments[i - 1] = new BSplineSegmentMatrix(points[i - 1], points[i], points[i + 1], points[i + 2]);
                else
                    m_BSplineSegments[i - 1] = new BSplineSegment(points[i - 1], points[i], points[i + 1], points[i + 2]);
            }
        }
    }
}
