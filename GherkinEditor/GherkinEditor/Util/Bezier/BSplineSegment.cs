using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    /// <summary>
    /// Cubic B-Spline
    /// Q(u, p0, p1, p2, p3) = B0*u^3 + B1*u^2 + B2*u + B3
    /// B0 = (-p0 + 3 * p1 - 3 * p2 + p3) / 6.0
    /// B1 = (3 * p0 - 6 * p1 + 3 * p2) / 6.0
    /// B2 = (-3 * p0 + 3 * p2) / 6.0
    /// B3 = (p0 + 4 * p1 + p2) / 6.0
    /// </summary>
    public class BSplineSegment : IBSpline
    {
        private double[] X = new double[4];
        private double[] Y = new double[4];

        public BSplineSegment(GPoint p0, GPoint p1, GPoint p2, GPoint p3)
        {
            CalcBSpline(p0, p1, p2, p3);
        }

        /// <summary>
        /// Get point within B-Spline
        /// x = x0*u^3 + x1*u^2 + x2*u + x3
        /// y = y0*u^3 + y1*u^2 + y2*u + y3
        /// </summary>
        /// <param name="u">[0, 1]</param>
        /// <returns></returns>
        public GPoint GetPoint(double u)
        {
            double x = (X[2] + u * (X[1] + u * X[0])) * u + X[3];
            double y = (Y[2] + u * (Y[1] + u * Y[0])) * u + Y[3];

            return new GPoint(x, y);
        }

        /// <summary>
        /// K = (x'y" - y'x") / (x'^2 + y'^2)^(3/2)
        /// x' = 3x0*u^2 + 2x1*u + x2
        /// x" = 6x0*u + 2x1
        /// y' and y" is similar to x' and x"
        /// 
        /// Note: In order to make turning right has plus value, we use
        /// K = (y'x" - x'y") / (x'^2 + y'^2)^(3/2)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double? Curvature(double u)
        {
            double x1 = FirstDerivative(X[0], X[1], X[2], u);
            double x2 = SecondDerivative(X[0], X[1], u);

            double y1 = FirstDerivative(Y[0], Y[1], Y[2], u);
            double y2 = SecondDerivative(Y[0], Y[1], u);
            double curvature = (y1 * x2 - x1 * y2) / Math.Pow(x1 * x1 + y1 * y1, 1.5);
            return curvature;
        }

        private double FirstDerivative(double x0, double x1, double x2, double u)
            // x' = 3x0*u^2 + 2x1*u + x2
            => 3.0 * x0 * u * u + 2 * x1 * u + x2;

        private double SecondDerivative(double x0, double x1, double u)
            // x" = 6x0*u + 2x1
            => 6 * x0 * u + 2 * x1;

        /// <summary>
        /// Calculating B-Spline
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        private void CalcBSpline(GPoint p0, GPoint p1, GPoint p2, GPoint p3)
        {
            X[0] = Z0(p0.X, p1.X, p2.X, p3.X);
            X[1] = Z1(p0.X, p1.X, p2.X);
            X[2] = Z2(p0.X, p2.X);
            X[3] = Z3(p0.X, p1.X, p2.X);

            Y[0] = Z0(p0.Y, p1.Y, p2.Y, p3.Y); 
            Y[1] = Z1(p0.Y, p1.Y, p2.Y);
            Y[2] = Z2(p0.Y, p2.Y);
            Y[3] = Z3(p0.Y, p1.Y, p2.Y);
        }

        private double Z0(double p0, double p1, double p2, double p3)
            => (-p0 + 3 * p1 - 3 * p2 + p3) / 6.0;

        private double Z1(double p0, double p1, double p2)
            => (3 * p0 - 6 * p1 + 3 * p2) / 6.0;

        private double Z2(double p0, double p2)
            => (-3 * p0 + 3 * p2) / 6.0;

        private double Z3(double p0, double p1, double p2)
            => (p0 + 4 * p1 + p2) / 6.0;
    }
}
