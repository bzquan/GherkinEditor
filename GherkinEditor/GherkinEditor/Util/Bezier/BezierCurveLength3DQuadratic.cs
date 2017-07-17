using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    /// <summary>
    /// Quadratic 3D Bézier curve length calculation
    /// </summary>
    public class BezierCurveLength3DQuadratic
    {
        protected const double InterpolationPrecision = 0.001; // (1mm)

        /// <summary>
        /// Start point.
        /// </summary>
        private V3D P0;
        /// <summary>
        /// Control point.
        /// </summary>
        private V3D P1;
        /// <summary>
        /// End point.
        /// </summary>
        private V3D P2;

        private double m_Length = -1;

        /// <summary>
        /// Creates a quadratic Bézier curve.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">Control point.</param>
        /// <param name="c">End point.</param>
        public BezierCurveLength3DQuadratic(V3D a, V3D b, V3D c) { P0 = a; P1 = b; P2 = c; }

        /// <summary>
        /// Gets the calculated length.
        /// https://github.com/HTD/FastBezier
        /// </summary>
        /// <remarks>
        /// Integral calculation by Dave Eberly, slightly modified for the edge case with colinear control point.
        /// See: http://www.gamedev.net/topic/551455-length-of-a-generalized-quadratic-bezier-curve-in-3d/
        /// </remarks>
        public double Length
        {
            get
            {
                if (m_Length >= 0.0) return m_Length;

                if (P0 == P2) 
                {
                    if (P0 == P1) return 0.0;
                    return (P0 - P1).Length;
                }
                if (P1 == P0 || P1 == P2) return (P0 - P2).Length;

                V3D A0 = P1 - P0;
                V3D A1 = P0 - 2.0 * P1 + P2;
                if (!A1.Zero)
                {
                    double c = 4.0 * A1.Dot(A1);
                    double b = 8.0 * A0.Dot(A1);
                    double a = 4.0 * A0.Dot(A0);
                    double q = 4.0 * a * c - b * b;
                    double twoCpB = 2.0 * c + b;
                    double sumCBA = c + b + a;
                    var l0 = (0.25 / c) * (twoCpB * Math.Sqrt(sumCBA) - b * Math.Sqrt(a));
                    double k1 = 2.0 * Math.Sqrt(c * sumCBA) + twoCpB;
                    double k2 = 2.0 * Math.Sqrt(c * a) + b;
                    if ((k1 <= 0.0) || (k2 <= 0.0)) return l0;

                    var l1 = (q / (8.0 * Math.Pow(c, 1.5))) * (Math.Log(k1) - Math.Log(k2));
                    m_Length = l0 + l1;
                }
                else
                {
                    m_Length = 2.0 * A0.Length;
                }

                return m_Length;
            }
        }

        /// <summary>
        /// Interpolated point at t : 0..1 position
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private V3D P(double t) => (1.0 - t) * (1.0 - t) * P0 + 2.0 * t * (1.0 - t) * P1 + t * t * P2;

        /// <summary>
        /// Gets the old slow and inefficient line interpolated length.
        /// </summary>
        public double InterpolatedLength
        {
            get
            {
                if (P0 == P2)
                {
                    if (P0 == P1) return 0;
                    return (P0 - P1).Length;
                }
                if (P1 == P0 || P1 == P2) return (P0 - P2).Length;
                double dt = InterpolationPrecision / (P2 - P0).Length;
                double length = 0.0;
                for (double t = dt; t < 1.0; t += dt) length += (P(t - dt) - P(t)).Length;
                return length;
            }
        }
    }
}
