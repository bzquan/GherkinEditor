using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    /// <summary>
    /// Cubic Bézier curve length calculation
    /// </summary>
    public class BezierCurveLength3DCubic
    {
        public static double InterpolationPrecision = 0.001; // (1mm)
        public static double LineInterpolationPrecision = 0.05; // (5cm)

        #region Optimization constants

        private static double Sqrt3 = Math.Sqrt(3d);
        private static double Div18Sqrt3 = 18d / Sqrt3;
        private static double OneThird = 1d / 3d;
        private static double Sqrt3Div36 = Sqrt3 / 36d;

        #endregion

        /// <summary>
        /// Start point.
        /// </summary>
        private V3D A;
        /// <summary>
        /// Control point 1.
        /// </summary>
        private V3D B;
        /// <summary>
        /// Control point 2.
        /// </summary>
        private V3D C;
        /// <summary>
        /// End point.
        /// </summary>
        private V3D D;

        private double m_Length = -1;

        /// <summary>
        /// Creates a cubic Bézier curve.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public BezierCurveLength3DCubic(V3D a, V3D b, V3D c, V3D d) { A = a; B = b; C = c; D = d; }

        /// <summary>
        /// Gets the calculated length of adaptive quadratic approximation.
        /// </summary>
        public double Length
        {
            get
            {
                if (m_Length >= 0.0) return m_Length;

                if (C == D)
                {
                    BezierCurveLength3DQuadratic lengCalc = new BezierCurveLength3DQuadratic(A, B, C);
                    m_Length = lengCalc.Length;
                    return m_Length;
                }

                double tmax = 0.0;
                BezierCurveLength3DCubic segment = this;
                List<BezierCurveLength3DCubic> segments = new List<BezierCurveLength3DCubic>();
                while ((tmax = segment.Tmax) < 1.0)
                {
                    var split = segment.SplitAt(tmax);
                    segments.Add(split[0]);
                    segment = split[1];
                }
                segments.Add(segment);
                m_Length = segments.Sum(s => s.QLength);
                return m_Length;
            }
        }

        /// <summary>
        /// Gets the length of the curve obtained via line interpolation.
        /// </summary>
        public double InterpolatedLength
        {
            get
            {
                double dt = LineInterpolationPrecision / (D - A).Length, length = 0.0;
                for (double t = dt; t < 1.0; t += dt) length += (P(t - dt) - P(t)).Length;
                return length;
            }
        }

        /// <summary>
        /// Interpolated point at t : 0..1 position.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private V3D P(double t) => A + 3.0 * t * (B - A) + 3.0 * t * t * (C - 2.0 * B + A) + t * t * t * (D - 3.0 * C + 3.0 * B - A);

        /// <summary>
        /// Gets the control point for the mid-point quadratic approximation.
        /// </summary>
        private V3D Q => (3.0 * C - D + 3.0 * B - A) / 4.0;

        /// <summary>
        /// Gets the mid-point quadratic approximation.
        /// </summary>
        private BezierCurveLength3DQuadratic M => new BezierCurveLength3DQuadratic(A, Q, D);

        /// <summary>
        /// Splits the curve at given position (t : 0..1).
        /// </summary>
        /// <param name="t">A number from 0 to 1.</param>
        /// <returns>Two curves.</returns>
        /// <remarks>
        /// (De Casteljau's algorithm, see: http://caffeineowl.com/graphics/2d/vectorial/bezierintro.html)
        /// </remarks>
        private BezierCurveLength3DCubic[] SplitAt(double t)
        {
            V3D a = V3D.Interpolate(A, B, t);
            V3D b = V3D.Interpolate(B, C, t);
            V3D c = V3D.Interpolate(C, D, t);
            V3D m = V3D.Interpolate(a, b, t);
            V3D n = V3D.Interpolate(b, c, t);
            V3D p = P(t);
            return new[] { new BezierCurveLength3DCubic(A, a, m, p), new BezierCurveLength3DCubic(p, n, c, D) };
        }

        /// <summary>
        /// Gets the distance between 0 and 1 quadratic aproximations.
        /// </summary>
        private double D01 => (D - 3.0 * C + 3.0 * B - A).Length / 2.0;

        /// <summary>
        /// Gets the split point for adaptive quadratic approximation.
        /// </summary>
        private double Tmax => Math.Pow(Div18Sqrt3 * InterpolationPrecision / D01, OneThird);

        /// <summary>
        /// Gets the calculated length of the mid-point quadratic approximation
        /// </summary>
        private double QLength => M.Length;
    }
}
