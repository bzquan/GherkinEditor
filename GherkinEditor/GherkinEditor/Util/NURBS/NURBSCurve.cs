using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.NURBS
{
    public class NURBSCurve
    {
        private List<GPoint> ControlPoints { get; set; }
        private int Degree { get; set; }    // Degree of spline -> p
        private double[] KnotVector;        // Knot vector -> U

        /// <summary>
        /// Create BURBSCurve
        /// </summary>
        /// <param name="points">control points</param>
        /// <param name="degree">degree of NURBS, order is p + 1</param>
        public NURBSCurve(List<GPoint> points, int degree)
        {
            ControlPoints = points;
            Degree = Math.Min(degree, points.Count - 1);
            KnotVector = NURBS.CalcualteKnotVector(degree, points.Count);
        }

        /// <summary>
        /// Non Rational B-Spline curve point
        /// </summary>
        /// <param name="u">interval parameter 0 ≦ u ≦　1</param>
        /// <returns></returns>
        public GPoint NonRationalBSpline(double u)
        {
            int span = NURBS.FindSpan(Degree, u, KnotVector);
            double[] basisFuns = NURBS.BasisFuns(span, Degree, KnotVector, u);
            double x = 0.0;
            double y = 0.0;
            for (int i = 0; i <= Degree; i++)
            {
                x += basisFuns[i] * ControlPoints[span - Degree + i].X;
                y += basisFuns[i] * ControlPoints[span - Degree + i].Y;
            }

            return new GPoint(x, y);
        }

        /// <summary>
        /// Rational B-Splines (NURBS) curve
        /// </summary>
        /// <param name="u">interval parameter 0 ≦ u ≦　1</param>
        /// <returns>curve point at u</returns>
        public GPoint RationalBSpline(double u)
        {
            int span = NURBS.FindSpan(Degree, u, KnotVector);
            double[] basisFuns = NURBS.BasisFuns(span, Degree, KnotVector, u);// CalcBasisFuns(span, u);

            double x = 0;
            double y = 0;
            double rationalWeight = 0.0;
            for (int i = 0; i <= Degree; i++)
            {
                GPoint cp = ControlPoints[span - Degree + i];
                x += basisFuns[i] * cp.X * cp.W;
                y += basisFuns[i] * cp.Y * cp.W;
                rationalWeight += basisFuns[i] * cp.W;
            }

            return new GPoint(x / rationalWeight, y / rationalWeight);
        }

        /// <summary>
        /// Curvature for Uniform Rational B-Spline curve
        /// An algorithm to compute the point on a B-spline curve and all derivatives up to and
        /// including the dth at a fixed u value
        /// Output is the array CK[], where CK [k] is the kth derivative, 0 ≦ k ≦ d.
        /// K = (x'y" - y'x") / (x'^2 + y'^2)^(3/2)
        /// Note: In order to make turning right has plus value, we use
        /// K = (y'x" - x'y") / (x'^2 + y'^2)^(3/2)
        /// Note: Calculating curvature for NURBS is complicated.
        /// </summary>
        /// <param name="u">interval parameter 0 ≦ u ≦　1</param>
        /// <returns>Curvature at u</returns>
        public double Curvature(double u)
        {
            if (Degree < 3) return 0;

            int du = 2;
            double[] CK_x = new double[du + 1];
            double[] CK_y = new double[du + 1];

            int span = NURBS.FindSpan(Degree, u, KnotVector);
            double[][] nders = NURBS.DerBasisFuns(span, u, Degree, du, KnotVector);
            for (int k = 1; k <= du; k++)
            {
                for (int j = 0; j <= Degree; j++)
                {
                    CK_x[k] += nders[k][j] * ControlPoints[span - Degree + j].X;
                    CK_y[k] += nders[k][j] * ControlPoints[span - Degree + j].Y;
                }
            }

            double curvature = (CK_y[1] * CK_x[2] - CK_x[1] * CK_y[2]) / Math.Pow(CK_x[1] * CK_x[1] + CK_y[1] * CK_y[1], 1.5);
            return curvature;
        }

        /// <summary>
        /// Tangent value at u
        /// </summary>
        /// <param name="u">interval parameter 0 ≦ u ≦　1</param>
        /// <returns>Tangent at u</returns>
        public double Tangent(double u)
        {
            if (Degree < 2) return 0;

            int span = NURBS.FindSpan(Degree, u, KnotVector);
            int du = 1;
            double[][] nders = NURBS.DerBasisFuns(span, u, Degree, du, KnotVector);
            double c_x = 0.0;
            double c_y = 0.0;
            for (int j = 0; j <= Degree; j++)
            {
                c_x += nders[1][j] * ControlPoints[span - Degree + j].X;
                c_y += nders[1][j] * ControlPoints[span - Degree + j].Y;
            }

            double tangent = c_y / c_x;
            return RoundTangent(tangent);
        }

        /// <summary>
        /// Tangent by degree
        /// </summary>
        /// <param name="u">interval parameter 0 ≦ u ≦　1</param>
        /// <returns>Tangent by degree in (-90..90]</returns>
        public double TangentByDegree(double u)
        {
            double tangent = Tangent(u);
            if (Math.Abs(tangent - NURBS.MAX_VALUE) < NURBS.EPSILON)
                return 90.0;

            double radians = Math.Atan(tangent);
            double angle = radians * (180.0 / Math.PI);

            return angle;
        }

        private double RoundTangent(double tangent)
        {
            if (NURBS.IsZero(tangent))
                return 0.0;
            else if (Math.Abs(tangent) > NURBS.MAX_VALUE)
                return NURBS.MAX_VALUE;
            else
                return tangent;
        }

        public static List<GPoint> CalcCurvature(List<GPoint> XY, int expectedCurvatureCount = 100)
        {
            int degree = XY.Count - 1;
            NURBSCurve bspline = new NURBSCurve(XY, degree);
            int point_num = expectedCurvatureCount;
            List<GPoint> curvaturePoints = new List<GPoint>(point_num);
            double step = 1.0 / point_num;
            for (double u = 0; u <= 1.0; u += step)
            {
                double? curvature = bspline.Curvature(u);
                if (curvature != null)
                {
                    double x = (int)(u * point_num * 100.0) / 100.0; // Keep only first 2 digits after the decimal point
                    curvaturePoints.Add(new GPoint(x, curvature.Value));
                }
            }

            return curvaturePoints;
        }
    }
}
