using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public enum CurvatureUnit { Meter, Centimeter }
    /// <summary>
    /// https://pomax.github.io/bezierinfo/
    /// https://github.com/ncareol/bspline
    /// </summary>
    public class BezierCurve
    {
        private static  List<List<double>> s_LookupTable = new List<List<double>>();
        private List<GPoint> m_Points;
        private List<GPoint> m_1stDerivative;
        private List<GPoint> m_2ndDerivative;

        public int BezierDegree => m_Points.Count - 1;

        /// <summary>
        /// Construct Bezier curve object
        /// </summary>
        /// <param name="points">sample points</param>
        public BezierCurve(List<GPoint> points)
        {
            m_Points = points;
        }

        /// <summary>
        /// Bezier(n,t,w[]) where n = w.length
        /// </summary>
        /// <param name="t">Bezier parameter</param>
        /// <returns></returns>
        public GPoint Bezier(double t)
        {
            double x = 0;
            double y = 0;
            int n = m_Points.Count - 1;
            for (int k = 0; k <= n; k++)
            {
                double base_bezier = Binomial(n, k) * Math.Pow(1 - t, n - k) * Math.Pow(t, k);
                x += m_Points[k].X * base_bezier; 
                y += m_Points[k].Y * base_bezier;
            }
            return new GPoint(x, y);
        }

        /// <summary>
        /// Calcuate curvature.
        /// </summary>
        /// <param name="i">the range is [0, m_Points.Count)</param>
        /// <returns></returns>
        public double Curvature(int i)
        {
            if ((i < 0) || (i >= m_Points.Count)) return 0.0;

            if (m_1stDerivative == null)
            {
                CalcDerivatives();
            }

            // Note: We use (P'' × P') here instead of (P' × P'') to let right turning has + curvature.
            double curvature = (m_2ndDerivative[i] * m_1stDerivative[i]) / Math.Pow(m_1stDerivative[i].SquareMagnitude(), 1.5);
            if (CurveViewCache.Instance.CurvatureUnit == CurvatureUnit.Centimeter)
                curvature /= 100; // Convert from 1/m to 1/cm

            return curvature;   
        }

        private void CalcDerivatives()
        {
            List<GPoint> diff1, diff2;
            CalcDifferential(out diff1, out diff2);

            int INTERVAL_NUM = m_Points.Count;
            m_1stDerivative = new List<GPoint>(INTERVAL_NUM);
            m_2ndDerivative = new List<GPoint>(INTERVAL_NUM);
            BezierCurve bezier1 = new BezierCurve(diff1);
            BezierCurve bezier2 = new BezierCurve(diff2);

            double step = 1.0 / INTERVAL_NUM;
            for (double t = 0; t < 1.0; t += step)
            {
                m_1stDerivative.Add(bezier1.Bezier(t));
                m_2ndDerivative.Add(bezier2.Bezier(t));
            }
        }

        private void CalcDifferential(out List<GPoint> diff1, out List<GPoint> diff2)
        {
            int n = m_Points.Count - 1;
            int m = n - 1;
            diff1 = new List<GPoint>(n);
            diff2 = new List<GPoint>(m);
            for (int i = 0; i < n; i++)
            {
                // for calculating 1st derivative
                GPoint p1 = (m_Points[i + 1] - m_Points[i]) * n;
                diff1.Add(p1);

                // for calculating 2nd derivative
                if (i > 0)
                {
                    GPoint p2 = (diff1[i] - diff1[i - 1]) * m;
                    diff2.Add(p2);
                }
            }
        }

        /// <summary>
        /// Make Pascal's triangle cache
        ///       [1],           // n=0                        C(0,0)
        ///      [1,1],          // n=1                    C(1,0), C(1,1)
        ///     [1,2,1],         // n=2                C(2,0), C(2,1), C(2,2)
        ///    [1,3,3,1],        // n=3            C(3,0), C(3,1), C(3,2), C(3,3)
        ///   [1,4,6,4,1],       // n=4        C(4,0), C(4,1), C(4,2), C(4,3), C(4,4)
        ///  [1,5,10,10,5,1],    // n=5    C(5,0), C(5,1), C(5,2), C(5,3), C(5,4), C(5,5)
        /// [1,6,15,20,15,6,1]   // n=6 C(6,0), C(6,1), C(6,2), C(6,3), C(6,4), C(6,5), C(6,6)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        private static double Binomial(int n, int k)
        {
            while (n >= s_LookupTable.Count)
            {
                int s = s_LookupTable.Count;
                List<double> nextRow = new List<double>(s + 1);
                nextRow.Resize(s + 1);
                nextRow[0] = 1;
                int prev = s - 1;
                for (int i = 1; i <= prev; i++)
                {
                    nextRow[i] = s_LookupTable[prev][i - 1] + s_LookupTable[prev][i];
                }
                nextRow[s] = 1;
                s_LookupTable.Add(nextRow);
            }

            return s_LookupTable[n][k];
        }
    }
}
