using System;
using System.Collections.Generic;
using System.Diagnostics;


/// <summary>
/// Simple cubic spline interpolation library without external dependencies
/// http://kluge.in-chemnitz.de/opensource/spline/
/// https://www.tangiblesoftwaresolutions.com/free_editions.html
/// </summary>
namespace Gherkin.Model
{
    /// <summary>
    /// spline interpolation
    /// </summary>
    public class CubicSpline
    {
        public enum SplineType { OriginalXY, ReversedXY }
        public enum bd_type
        {
            first_deriv = 1,
            second_deriv = 2
        }

        private enum XYOrder { OriginalX, ReverseX, OriginalY, ReverseY }

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
        /// <param name="force_linear_extrapolation"></param>
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
        public void PreparePoints(List<double> x, List<double> y)
        {
            if (IsOriginal(x))
            {
                m_XYOrder = XYOrder.OriginalX;
                SetPoints(x, y);
            }
            else if (IsReverse(x))
            {
                m_XYOrder = XYOrder.ReverseX;
                x.Reverse();
                y.Reverse();
                SetPoints(x, y);
            }
            else if (IsOriginal(y))
            {
                m_XYOrder = XYOrder.OriginalY;
                SetPoints(y, x);
            }
            else
            {
                m_XYOrder = XYOrder.ReverseY;
                x.Reverse();
                y.Reverse();
                SetPoints(y, x);
            }
        }

        /// <summary>
        /// Calculate point on spline value
        /// Candidate point(X, Y):
        /// Y will be calculated by X if SplineKind is OriginalXY,
        /// or X will be calculated by Y if SplineKind is ReversedXY
        /// </summary>
        /// <param name="p">candidate point </param>
        /// <returns></returns>
        public OxyPlot.DataPoint CalculateY(OxyPlot.DataPoint p)
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

        private bool IsOriginal(List<double> x)
        {
            for (int i = 0; i < x.Count - 1; i++)
            {
                if (x[i] >= x[i + 1]) return false;
            }
            return true;
        }
        private bool IsReverse(List<double> x)
        {
            for (int i = 0; i < x.Count - 1; i++)
            {
                if (x[i] <= x[i + 1]) return false;
            }
            return true;
        }

        private void SetPoints(List<double> x, List<double> y)
        {
            Debug.Assert(x.Count == y.Count);
            Debug.Assert(x.Count > 2);

            m_x = new List<double>(x);
            m_y = new List<double>(y);
            int n = x.Count;

            // This is precondition for this algorithm
            // Debug.Assert(IsStrictlyOrdered(m_x));

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
    }
}


