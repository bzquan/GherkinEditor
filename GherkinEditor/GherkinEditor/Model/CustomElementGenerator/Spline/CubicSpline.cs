using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gherkin.Model
{
    /// <summary>
    /// spline interpolation
    /// Simple cubic spline interpolation library without external dependencies
    /// http://kluge.in-chemnitz.de/opensource/spline/
    /// https://www.tangiblesoftwaresolutions.com/free_editions.html
    /// https://www.codeproject.com/Articles/25237/Bezier-Curves-Made-Simple
    /// </summary>
    public class CubicSpline
    {
        public enum SplineType { OriginalXY, ReversedXY }
        public enum bd_type
        {
            first_deriv = 1,
            second_deriv = 2
        }

        private enum XYOrder { OriginalX, ReverseX, OriginalY, ReverseY, None }

        private List<double> m_x = new List<double>(); // x,y coordinates of points
        private List<double> m_y = new List<double>();
        // interpolation parameters
        // f(x) = a*(x-x_i)^3 + b*(x-x_i)^2 + c*(x-x_i) + y_i
        private List<double> m_a = new List<double>(); // spline coefficients
        private List<double> m_b = new List<double>();
        private List<double> m_c = new List<double>();
        private double m_b0; // for left extrapol
        private double m_c0;
        private bd_type m_left;
        private bd_type m_right;
        private double m_left_value;
        private double m_right_value;
        private XYOrder m_XYOrder = XYOrder.OriginalX;

        public SplineType SplineKind()
        {
            if (m_XYOrder == XYOrder.OriginalX || (m_XYOrder == XYOrder.ReverseX))
                return SplineType.OriginalXY;
            else
                return SplineType.ReversedXY;
        }

        // set default boundary condition to be zero curvature at both ends
        public CubicSpline()
        {
            this.m_left = CubicSpline.bd_type.second_deriv;
            this.m_right = CubicSpline.bd_type.second_deriv;
            this.m_left_value = 0.0;
            this.m_right_value = 0.0;
        }

        /// <summary>
        /// optional, but if called it has to come be before PreparePoints()
        /// </summary>
        /// <param name="left"></param>
        /// <param name="left_value"></param>
        /// <param name="right"></param>
        /// <param name="right_value"></param>
        public void SetBoundary(CubicSpline.bd_type left, double left_value, CubicSpline.bd_type right, double right_value)
        {
            Debug.Assert(m_x.Count == 0); // PreparePoints() must not have happened yet
            m_left = left;
            m_right = right;
            m_left_value = left_value;
            m_right_value = right_value;
        }

        /// <summary>
        /// Prepare sample data for calculating spline.
        /// Prerequesite: X or Y should satisfy one of the following condition
        /// 1. X is ordered by increasing strictly
        /// 2. X is ordered by decreasing strictly
        /// 3. Y is ordered by increasing strictly
        /// 4. Y is ordered by decreasing strictly
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool PreparePoints(List<double> x, List<double> y)
        {
            m_XYOrder = CheckXYOrder(x, y);
            switch (m_XYOrder)
            {
                case XYOrder.OriginalX:
                    SetPoints(x, y);
                    break;
                case XYOrder.ReverseX:
                    x.Reverse();
                    y.Reverse();
                    SetPoints(x, y);
                    break;
                case XYOrder.OriginalY:
                    SetPoints(y, x);
                    break;
                case XYOrder.ReverseY:
                    x.Reverse();
                    y.Reverse();
                    SetPoints(y, x);
                    break;
            }

            return (m_XYOrder != XYOrder.None);
        }

        /// <summary>
        /// Calculate point on spline value
        /// Candidate point(X, Y):
        /// Y will be calculated by X if SplineKind is OriginalXY,
        /// or X will be calculated by Y if SplineKind is ReversedXY
        /// </summary>
        /// <param name="p">candidate point </param>
        /// <returns></returns>
        public OxyPlot.DataPoint CalcXorY(GPoint p)
        {
            int n = m_x.Count;
            // find the closest point m_x[idx] < x, idx=0 even if x<m_x[0]
            //std::vector<double>::const_iterator it;
            //it = std::lower_bound(m_x.begin(), m_x.end(), x);
            //int idx = std::max(int(it - m_x.begin()) - 1, 0);
            double x = (SplineKind() == SplineType.OriginalXY) ? p.X : p.Y;
            int idx = LowerBound(m_x, x);
            double h = x - m_x[idx];
            double interpol;
            if (x < m_x[0])
            {
                // extrapolation to the left
                interpol = (m_b0 * h + m_c0) * h + m_y[0];
            }
            else if (x > m_x[n - 1])
            {
                // extrapolation to the right
                interpol = (m_b[n - 1] * h + m_c[n - 1]) * h + m_y[n - 1];
            }
            else
            {
                // interpolation
                interpol = ((m_a[idx] * h + m_b[idx]) * h + m_c[idx]) * h + m_y[idx];
            }

            OxyPlot.DataPoint spline_point = (SplineKind() == SplineType.OriginalXY) ?
                                            new OxyPlot.DataPoint(x, interpol) : new OxyPlot.DataPoint(interpol, x);

            return spline_point;
        }

        /// <summary>
        /// Calculate curvature at x on spline
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double CalculateCurvature(double x)
        {
            int n = m_x.Count;
            // find the closest point m_x[idx] < x, idx=0 even if x<m_x[0]
            //std::vector<double>::const_iterator it;
            //it = std::lower_bound(m_x.begin(), m_x.end(), x);
            //int idx = std::max(int(it - m_x.begin()) - 1, 0);
            int idx = LowerBound(m_x, x);
            if (x < m_x[0])
            {
                // use left most equation
                return CalcCurvature(0, m_b0, m_c0, x);
            }
            else if (x > m_x[n - 1])
            {
                // use right most equation
                return CalcCurvature(0, m_b[n - 1], m_c[n - 1], x);
            }
            else
            {
                return CalcCurvature(m_a[idx], m_b[idx], m_c[idx], x);
            }
        }

        private XYOrder CheckXYOrder(List<double> x, List<double> y)
        {
            if ((x.Count == 0) || (x.Count != y.Count)) return XYOrder.None;

            bool IsOriginalX = true;
            bool IsReverseX = true;
            bool IsOriginalY = true;
            bool IsReverseY = true;
            
            for (int i = 0; i < x.Count -1; i++)
            {
                IsOriginalX = IsOriginalX && (x[i] < x[i + 1]);
                IsReverseX = IsReverseX && (x[i] > x[i + 1]);
                IsOriginalY = IsOriginalY && (y[i] < y[i + 1]);
                IsReverseY = IsReverseY && (y[i] > y[i + 1]);
            }

            if (IsOriginalX) return XYOrder.OriginalX;
            if (IsReverseX) return XYOrder.ReverseX;
            if (IsOriginalY) return XYOrder.OriginalY;
            if (IsReverseY) return XYOrder.ReverseY;

            return XYOrder.None;
        }

        private void SetPoints(List<double> x, List<double> y)
        {
            Debug.Assert(x.Count == y.Count);
            Debug.Assert(x.Count > 2);

            m_x = new List<double>(x);
            m_y = new List<double>(y);
            int n = x.Count;

            // cubic spline interpolation
            // setting up the matrix and right hand side of the equation system
            // for the parameters b[]
            BandMatrix A = new BandMatrix(n, 1, 1);
            List<double> rhs = new List<double>(n);
            rhs.Resize(n);
            for (int i = 1; i < n - 1; i++)
            {
                A[i, i - 1] = 1.0 / 3.0 * (x[i] - x[i - 1]);
                A[i, i] = 2.0 / 3.0 * (x[i + 1] - x[i - 1]);
                A[i, i + 1] = 1.0 / 3.0 * (x[i + 1] - x[i]);
                rhs[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]) - (y[i] - y[i - 1]) / (x[i] - x[i - 1]);
            }
            // boundary conditions
            if (m_left == CubicSpline.bd_type.second_deriv)
            {
                // 2*b[0] = f''
                A[0, 0] = 2.0;
                A[0, 1] = 0.0;
                rhs[0] = m_left_value;
            }
            else if (m_left == CubicSpline.bd_type.first_deriv)
            {
                // c[0] = f', needs to be re-expressed in terms of b:
                // (2b[0]+b[1])(x[1]-x[0]) = 3 ((y[1]-y[0])/(x[1]-x[0]) - f')
                A[0, 0] = 2.0 * (x[1] - x[0]);
                A[0, 1] = 1.0 * (x[1] - x[0]);
                rhs[0] = 3.0 * ((y[1] - y[0]) / (x[1] - x[0]) - m_left_value);
            }
            else
            {
                Debug.Assert(false);
            }
            if (m_right == CubicSpline.bd_type.second_deriv)
            {
                // 2*b[n-1] = f''
                A[n - 1, n - 1] = 2.0;
                A[n - 1, n - 2] = 0.0;
                rhs[n - 1] = m_right_value;
            }
            else if (m_right == CubicSpline.bd_type.first_deriv)
            {
                // c[n-1] = f', needs to be re-expressed in terms of b:
                // (b[n-2]+2b[n-1])(x[n-1]-x[n-2])
                // = 3 (f' - (y[n-1]-y[n-2])/(x[n-1]-x[n-2]))
                A[n - 1, n - 1] = 2.0 * (x[n - 1] - x[n - 2]);
                A[n - 1, n - 2] = 1.0 * (x[n - 1] - x[n - 2]);
                rhs[n - 1] = 3.0 * (m_right_value - (y[n - 1] - y[n - 2]) / (x[n - 1] - x[n - 2]));
            }
            else
            {
                Debug.Assert(false);
            }

            // solve the equation system to obtain the parameters b[]
            m_b = new List<double>(A.LUSolve(rhs));

            // calculate parameters a[] and c[] based on b[]
            m_a.Resize(n);
            m_c.Resize(n);
            for (int i = 0; i < n - 1; i++)
            {
                m_a[i] = 1.0 / 3.0 * (m_b[i + 1] - m_b[i]) / (x[i + 1] - x[i]);
                m_c[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]) - 1.0 / 3.0 * (2.0 * m_b[i] + m_b[i + 1]) * (x[i + 1] - x[i]);
            }

            // for left extrapolation coefficients
            m_b0 = m_b[0];
            m_c0 = m_c[0];

            // for the right extrapolation coefficients
            // f_{n-1}(x) = b*(x-x_{n-1})^2 + c*(x-x_{n-1}) + y_{n-1}
            double h = x[n - 1] - x[n - 2];
            // m_b[n-1] is determined by the boundary condition
            m_a[n - 1] = 0.0;
            m_c[n - 1] = 3.0 * m_a[n - 2] * h * h + 2.0 * m_b[n - 2] * h + m_c[n - 2]; // = f'_{n-2}(x_{n-1})
        }

        private static int LowerBound(List<double> sortedList, double x)
        {
            int lower_bound = 0;
            for (int i = 0; i < sortedList.Count; i++)
            {
                if (sortedList[i] < x)
                    lower_bound = i;
                else
                    break; 
            }

            return lower_bound;
        }

        /// <summary>
        /// Calculate curvature at x for cubic equation
        /// f(x) = a*x^3 + b*x^2 + c*x + d
        /// Note:
        ///  curvature = f''(x) / (1 + f'(x) ^ 2) ^(3/2)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double CalcCurvature(double a, double b, double c, double x)
        {
            double numer = 6.0 * a * x + 2.0 * b;
            double f = 3.0 * a * Math.Pow(x, 2) + 2.0 * b * x + c;
            double denom = Math.Pow(1.0 + Math.Pow(f, 2), 1.5);

            return (-1 * (numer / denom) / 100.0);  // 正は右曲り
        }
    }
}


