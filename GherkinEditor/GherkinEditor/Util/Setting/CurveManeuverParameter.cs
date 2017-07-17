using System;

namespace Gherkin.Util
{
    public enum CurvatureUnit { Meter, Centimeter }

    /// <summary>
    /// Paramer for calculating maneuver of a curve
    /// </summary>
    public class CurveManeuverParameter
    {
        private int m_NURBSDegree = 50; // NURBS degree

        public CurvatureUnit Unit { get; set; } = CurvatureUnit.Meter;
        public double CurveMinDistance { get; set; } = 30;              // distance tolerance is 30m
        public double DouglasPeuckerTolerance { get; set; } = 0.002;    // for cm 0.00002
        public double CurveCurvatureThreshold { get; set; } = 0.002;    // Radius = 500m, for cm 0.00002
        public double YTolerance { get; set; } = 0.0025;                // for cm 0.000025
        public bool ThinoutByDouglasPeuckerN { get; set; } = false;     // Thin out input points by using variant Douglas-Peucker algorithm
        public int NURBSDegree
        {
            get { return m_NURBSDegree; }
            set { m_NURBSDegree = Math.Max(3, value); }
        }
    }
}
