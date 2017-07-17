using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetMatrix;
using Gherkin.Util.Geometric;

namespace Gherkin.Util.Bezier
{
    public class BSplineSegmentMatrix : IBSpline
    {
        private static readonly Lazy<GeneralMatrix> s_M =
            new Lazy<GeneralMatrix>(() => InitializeMatrix());
        
        private GeneralMatrix MX;
        private GeneralMatrix MY;

        public BSplineSegmentMatrix(GPoint p0, GPoint p1, GPoint p2, GPoint p3)
        {
            CalcMPMatrix(p0, p1, p2, p3);
        }

        public GPoint GetPoint(double u)
        {
            GeneralMatrix uv = new GeneralMatrix(new double[] { 1, u, u * u, u * u * u }, 1);
            double x = (uv * MX).GetElement(0, 0);
            double y = (uv * MY).GetElement(0, 0);

            return new GPoint(x, y);
        }

        /// <summary>
        /// K = (x'y" - y'x") / (x'^2 + y'^2)^(3/2)
        /// 
        /// Note: In order to make turning right has plus value, we use
        /// K = (y'x" - x'y") / (x'^2 + y'^2)^(3/2)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double? Curvature(double u)
        {
            GeneralMatrix d1 = new GeneralMatrix(new double[] { 0, 1, 2 * u, 3 * u * u }, 1);
            GeneralMatrix d2 = new GeneralMatrix(new double[] { 0, 0, 2, 6 * u }, 1);
            var k = d1 * MX;
            double x1 = (d1 * MX).GetElement(0, 0);
            double y1 = (d1 * MY).GetElement(0, 0);
            double x2 = (d2 * MX).GetElement(0, 0);
            double y2 = (d2 * MY).GetElement(0, 0);

            double curvature = (y1 * x2 - x1 * y2) / Math.Pow(x1 * x1 + y1 * y1, 1.5);
            return curvature;
        }

        /// <summary>
        /// Calculating B-Spline
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        private void CalcMPMatrix(GPoint p0, GPoint p1, GPoint p2, GPoint p3)
        {
            GeneralMatrix x = new GeneralMatrix(new double[] { p0.X, p1.X, p2.X, p3.X }, 4);
            GeneralMatrix y = new GeneralMatrix(new double[] { p0.Y, p1.Y, p2.Y, p3.Y }, 4);

            MX = s_M.Value * x;
            MY = s_M.Value * y;
        }

        private static GeneralMatrix InitializeMatrix()
        {
            var M = new GeneralMatrix( new double[][]{
                new double[]{1,  4,  1, 0},
                new double[]{-3, 0,  3, 0},
                new double[]{3, -6,  3, 0},
                new double[]{-1, 3, -3, 1}
            });

            return M * (1.0 / 6.0);
        }
    }
}
