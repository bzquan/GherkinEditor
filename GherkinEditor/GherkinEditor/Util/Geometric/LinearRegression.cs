using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util.Geometric
{
    /// <summary>
    /// Linear function: y = slope * x + intercept
    /// </summary>
    public struct LinearFunction
    {
        public double Slope { get; set; }
        public double Intercept { get; set; }
        /// <summary>
        /// CorrelationCoeff, i.e.c R, is always between –1 and 1
        /// Correlation coefficients closer to 1 or –1 indicate a stronger linear relationship
        /// Correlation coefficients close to 0 indicate a weak linear relationship.
        ///    Note: However there could be a nonlinear relationship when the correlation coefficient is close to 0.
        /// </summary>
        public double CorrelationCoeff { get; set; }
    }

    public static class LinearRegression
    {
        public static bool Regression(GPoint[] points, out LinearFunction linearFunc)
        {
            linearFunc = new LinearFunction();
            if (points.Length == 0) return false;

            double sumx = 0.0;  // sum of x
            double sumx2 = 0.0; // sum of x**2
            double sumxy = 0.0; // sum of x * y
            double sumy = 0.0;  // sum of y
            double sumy2 = 0.0; // sum of y**2

            for (int i = 0; i < points.Length; i++)
            {
                sumx += points[i].X;
                sumx2 += Sqr(points[i].X);
                sumxy += points[i].X * points[i].Y;
                sumy += points[i].Y;
                sumy2 += Sqr(points[i].Y);
            }

            double denom = (points.Length * sumx2 - Sqr(sumx));
            if (Math.Abs(denom) < double.Epsilon)
            {
                // singular matrix. can't solve the problem.
                return false;
            }

            linearFunc.Slope = (points.Length * sumxy - sumx * sumy) / denom;
            linearFunc.Intercept = (sumy * sumx2 - sumx * sumxy) / denom;
            linearFunc.CorrelationCoeff = (sumxy - sumx * sumy / points.Length) /
                                            Math.Sqrt((sumx2 - Sqr(sumx) / points.Length) * (sumy2 - Sqr(sumy) / points.Length));

            return true;
        }

        private static double Sqr(double x) => x * x;
    }
}
