using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util.NURBS
{
    /// <summary>
    /// Calculate NURBS
    /// Remark: "The NURBS Book" by Les Piegl and Wayne Tiller 
    /// Ref:
    /// https://www.codeproject.com/Articles/1095142/Generate-and-understand-NURBS-curves
    /// http://www.nar-associates.com/nurbs/c_code.html
    /// </summary>
    public static class NURBS
    {
        public const double EPSILON = 1.0E-15;
        public const double MAX_VALUE = 1.0E15;

        /// <summary>
        /// Determine the knot span index
        /// n = m - p - 1
        /// m : U.length - 1
        /// </summary>
        /// <param name="p">Order((p - 1) degree of B-Spline)</param>
        /// <param name="u">knot</param>
        /// <param name="U">knot vector</param>
        /// <returns>the knot span index</returns>
        public static int FindSpan(int p, double u, double[] U)
        {
            int m = U.Length - 1;

            // Do binary search
            int low = p;
            int high = m;
            int mid = (low + high) / 2;
            while ((u < U[mid]) || (u >= U[mid + 1]))
            {
                if (u < U[mid])
                    high = mid;
                else
                    low = mid;
                int next_mid = (low + high) / 2;
                if (next_mid == mid) break;

                mid = next_mid;
            }

            mid = Math.Max(mid, p);
            return Math.Min(mid, m - p - 1);
        }

        /// <summary>
        /// computes all the nonvanishing basis functions and stores them in the array N[O] , ... ,N[p].
        /// Note: on page 70 in "The NURBS Book"
        /// Remark: It is not only efficient, but it also guarantees that there will be no division by zero.
        /// </summary>
        /// <param name="i">Current control pont</param>
        /// <param name="p">The picewise polynomial degree</param>
        /// <param name="U">The knot vector</param>
        /// <param name="u">The value of the current curve point. Valid range from 0 ≦ u ≦　1</param>
        /// <returns>basis functions N[0..p]</returns>
        public static double[] BasisFuns(int i, int p, double[] U, double u)
        {
            double[] N = new double[p + 1];
            N[0] = 1.0;
            double[] left = new double[p + 1];
            double[] right = new double[p + 1];
            for (int j = 1; j <= p; j++)
            {
                left[j] = u - U[i + 1 - j];
                right[j] = U[i + j] - u;
                double saved = 0.0;
                for (int r=0; r<j; r++)
                {
                    double temp= N[r] / (right[r + 1] + left[j - r]);
                    N[r] = saved + right[r + 1] * temp;
                    saved = left[j - r] * temp;
                }
                N[j] = saved;
            }

            return N;
        }

        /// <summary>
        /// This code is translated to C# from the original C++  code given on page 74-75 in "The NURBS Book" by Les Piegl and Wayne Tiller 
        /// Remark: It calculate ONLY one basis function. It is only for testing.
        /// Dot not use it in production code because of low performance
        /// </summary>
        /// <param name="i">Current control pont</param>
        /// <param name="p">The picewise polynomial degree</param>
        /// <param name="U">The knot vector</param>
        /// <param name="u">The value of the current curve point. Valid range from 0 ≦ u ≦　1</param>
        /// <returns>N_{i,p}(u)</returns>
        public static double Nip(int i, int p, double[] U, double u)
        {
            double[] N = new double[p + 1];
            double saved, temp;

            int m = U.Length - 1;
            if ((i == 0 && IsEqual(u, U[0])) || (i == (m - p - 1) && IsEqual(u, U[m])))
                return 1.0;

            if (u < U[i] || u >= U[i + p + 1])
                return 0.0;

            for (int j = 0; j <= p; j++)
            {
                if (u >= U[i + j] && u < U[i + j + 1])
                    N[j] = 1.0;
                else
                    N[j] = 0.0;
            }

            for (int k = 1; k <= p; k++)
            {
                if (IsZero(N[0]))
                    saved = 0.0;
                else
                    saved = ((u - U[i]) * N[0]) / (U[i + k] - U[i]);

                for (int j = 0; j < p - k + 1; j++)
                {
                    double Uleft = U[i + j + 1];
                    double Uright = U[i + j + k + 1];

                    if (IsZero(N[j + 1]))
                    {
                        N[j] = saved;
                        saved = 0.0;
                    }
                    else
                    {
                        temp = N[j + 1] / (Uright - Uleft);
                        N[j] = saved + (Uright - u) * temp;
                        saved = (u - Uleft) * temp;
                    }
                }
            }
            return N[0];
        }

        /// <summary>
        /// This code is translated to C# from the original C++  code given on page 72-73 in "The NURBS Book" by Les Piegl and Wayne Tiller 
        /// Computes the nonzero basis functions and their derivatives,
        /// up to and including the nth derivative(n ≦ p).
        /// Output is in the two-dimensional array, ders. ders[k][j] is the kth derivative of the function Ni-p+j,p,
        /// where 0 ≦ k ≦ n and 0 ≦ j ≦ p. Two local arrays are used:
        /// ndu[p+l][p+l], to store the basis functions and knot differences
        /// a[2] [p+l], to store (in an alternating fashion) the two most recently computed rows a_k,j and a_k-1,j·
        /// The algorithm avoids division by zero and/or the use of terms not in the array ndu[][]
        /// </summary>
        /// <param name="i">Current control pont</param>
        /// <param name="u">The value of the current curve point. Valid range from 0 ≦ u ≦　1 </param>
        /// <param name="p">The picewise polynomial degree</param>
        /// <param name="n">derivative up to n</param>
        /// <param name="U">The knot vector</param>
        /// <returns>ders[n + 1][p + 1]</returns>
        public static double[][] DerBasisFuns(int i, double u, int p, int n, double[] U)
        {
            // initialize ders
            double[][] ders = new double[n + 1][];
            for (int k = 0; k < n + 1; k++) ders[k] = new double[p + 1];

            // initialize ndu;
            double[][] ndu = new double[p + 1][];
            for (int k = 0; k < p + 1; k++) ndu[k] = new double[p + 1];

            // initialize a
            double[][] a = new double[2][];
            for (int k = 0; k < 2; k++) a[k] = new double[p + 1];

            ndu[0][0] = 1.0;
            double[] left = new double[p + 1];
            double[] right = new double[p + 1];
            for (int j = 1; j <= p; j++)
            {
                left[j] = u - U[i + 1 - j];
                right[j] = U[i + j] - u;
                double saved = 0.0;
                for (int r = 0; r < j; r++)
                {
                    // lower triangle
                    ndu[j][r] = right[r + 1] + left[j - r];
                    double temp = ndu[r][j - 1] / ndu[j][r];
                    // upper triangle
                    ndu[r][j] = saved + right[r + 1] * temp;
                    saved = left[j - r] * temp;
                }
                ndu[j][j] = saved;
            }

            // Load the basis functions
            for (int j = 0; j <= p; j++)
                ders[0][j] = ndu[j][p];

            // This section computes the derivatives
            for (int r = 0; r <= p; r++)    // Loop over function index
            {
                int s1 = 0;
                int s2 = 1; // Alternate rows in array a
                a[0][0] = 1.0;

                // Loop to compute kth derivative
                for (int k = 1; k <= n; k++)
                {
                    double d = 0.0;
                    int rk = r - k;
                    int pk = p - k;
                    if (r >= k)
                    {
                        a[s2][0] = a[s1][0] / ndu[pk + 1][rk];
                        d = a[s2][0] * ndu[rk][pk];
                    }
                    int j1 = (rk >= -1) ? 1 : -rk;
                    int j2 = (r - 1 <= pk) ? k - 1 : p - r;
                    for (int j = j1; j <= j2; j++)
                    {
                        a[s2][j] = (a[s1][j] - a[s1][j - 1]) / ndu[pk + 1][rk + j];
                        d += a[s2][j] * ndu[rk + j][pk];
                    }
                    if (r <= pk)
                    {
                        a[s2][k] = -a[s1][k - 1] / ndu[pk + 1][r];
                        d += a[s2][k] * ndu[r][pk];
                    }
                    ders[k][r] = d;
                    // Switch rows
                    int temp = s1;
                    s1 = s2;
                    s2 = temp;
                }
            }

            // Multiply through by the correct factors
            int rr = p;
            for (int k = 1; k <= n; k++)
            {
                for (int j = 0; j <= p; j++) ders[k][j] *= rr;
                rr *= (p - k);
            }

            return ders;
        }

        /// <summary>
        /// Calculate knot vector
        /// </summary>
        /// <param name="degree">Degree of SPline</param>
        /// <param name="cpCount">count of control points</param>
        /// <returns></returns>
        public static double[] CalcualteKnotVector(int degree, int cpCount)
        {
            int n = cpCount;
            int m = n + degree + 1;
            double[] knots = new double[m];

            if (degree + 1 > cpCount || cpCount == 0)
                return knots;


            int divisor = m - 1 - 2 * degree;
            for (int i = 0; i < m; i++)
            {
                if (i <= degree)
                    knots[i] = 0.0;
                else if (i >= m - degree - 1)
                    knots[i] = 1.0;
                else
                {
                    double dividend = i - degree;
                    knots[i] = dividend / divisor;
                }
            }

            return knots;
        }

        public static bool IsEqual(double v1, double v2) => Math.Abs(v1 - v2) < EPSILON;
        public static bool IsZero(double v) => Math.Abs(v) < EPSILON;
    }
}
