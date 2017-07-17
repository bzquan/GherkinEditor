using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    /// <summary>
    /// Quadratic 2D Bézier curve length calculation
    /// </summary>
    public static class BezierCurveLength2DQuadratic
    {
        /// <summary>
        /// Gets the calculated length.
        /// https://github.com/HTD/FastBezier
        /// </summary>
        /// <remarks>
        /// Integral calculation by Dave Eberly, slightly modified for the edge case with colinear control point.
        /// See: http://www.gamedev.net/topic/551455-length-of-a-generalized-quadratic-bezier-curve-in-3d/
        /// </remarks>
        public static double Length(GPoint P0, GPoint P1, GPoint P2)
        {

            if (P0 == P2)
            {
                if (P0 == P1) return 0.0;
                return P0.Distance(P1);
            }
            if (P1 == P0 || P1 == P2) return P0.Distance(P2);

            GPoint A0 = P1 - P0;
            GPoint A1 = P0 - 2.0 * P1 + P2;
            if (!A1.Zero)
            {
                double c = 4.0 * A1.DotProduct(A1);
                double b = 8.0 * A0.DotProduct(A1);
                double a = 4.0 * A0.DotProduct(A0);
                double q = 4.0 * a * c - b * b;
                double twoCpB = 2.0 * c + b;
                double sumCBA = c + b + a;
                var l0 = (0.25 / c) * (twoCpB * Math.Sqrt(sumCBA) - b * Math.Sqrt(a));
                double k1 = 2.0 * Math.Sqrt(c * sumCBA) + twoCpB;
                double k2 = 2.0 * Math.Sqrt(c * a) + b;
                if ((k1 <= 0.0) || (k2 <= 0.0)) return l0;

                var l1 = (q / (8.0 * Math.Pow(c, 1.5))) * (Math.Log(k1) - Math.Log(k2));
                return l0 + l1;
            }
            else
            {
                return 2.0 * A0.Length();
            }
        }
    }
}
