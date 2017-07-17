using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util.Spline
{
    /// <summary>
    /// band matrix solver
    /// </summary>
    public class BandMatrix
    {
        private List<List<double>> m_upper = new List<List<double>>(); // upper band
        private List<List<double>> m_lower = new List<List<double>>(); // lower band

        public BandMatrix(int dim, int n_u, int n_l)
        {
            resize(dim, n_u, n_l);
        }

        public void resize(int dim, int n_u, int n_l)
        {
            Debug.Assert(dim > 0);
            Debug.Assert(n_u >= 0);
            Debug.Assert(n_l >= 0);
            m_upper.Resize(n_u + 1);
            m_lower.Resize(n_l + 1);
            for (int i = 0; i < m_upper.Count; i++)
            {
                m_upper[i] = new List<double>();
                m_upper[i].Resize(dim);
            }
            for (int i = 0; i < m_lower.Count; i++)
            {
                m_lower[i] = new List<double>();
                m_lower[i].Resize(dim);
            }
        }

        private int Dim()
        {
            if (m_upper.Count > 0)
            {
                return m_upper[0].Count;
            }
            else
            {
                return 0;
            }
        }

        private int NumUpper()
        {
            return m_upper.Count - 1;
        }

        private int NumLower()
        {
            return m_lower.Count - 1;
        }

        /// <summary>
        /// defines the indexer, so that we can access the elements
        /// by A[i,j], index going from i=0,...,dim()-1
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public double this[int i, int j]
        {
            get
            {
                int k = j - i; // what band is the entry
                Debug.Assert((i >= 0) && (i < Dim()) && (j >= 0) && (j < Dim()));
                Debug.Assert((-NumLower() <= k) && (k <= NumUpper()));
                // k=0 -> diogonal, k<0 lower left part, k>0 upper right part
                if (k >= 0)
                {
                    return m_upper[k][i];
                }
                else
                {
                    return m_lower[-k][i];
                }
            }
            set
            {
                int k = j - i; // what band is the entry
                Debug.Assert((i >= 0) && (i < Dim()) && (j >= 0) && (j < Dim()));
                Debug.Assert((-NumLower() <= k) && (k <= NumUpper()));
                // k=0 -> diogonal, k<0 lower left part, k>0 upper right part
                if (k >= 0)
                {
                    m_upper[k][i] = value;
                }
                else
                {
                    m_lower[-k][i] = value;
                }
            }
        }

        /// <summary>
        /// saved_diag
        /// we can store an additional diogonal (in m_lower)
        /// second diag (used in LU decomposition), saved in m_lower
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double this[int i]
        {
            get
            {
                Debug.Assert((i >= 0) && (i < Dim()));
                return m_lower[0][i];
            }
            set
            {
                Debug.Assert((i >= 0) && (i < Dim()));
                m_lower[0][i] = value;
            }
        }

        /// <summary>
        /// LR-Decomposition of a band matrix
        /// </summary>
        private void LUDecompose()
        {
            int i_max;
            int j_max;
            int j_min;
            double x;

            // preconditioning
            // normalize column i so that a_ii=1
            for (int i = 0; i < this.Dim(); i++)
            {
                Debug.Assert(this[i, i] != 0.0);
                this[i] = 1.0 / this[i, i];
                j_min = Math.Max(0, i - this.NumLower());
                j_max = Math.Min(this.Dim() - 1, i + this.NumUpper());
                for (int j = j_min; j <= j_max; j++)
                {
                    this[i, j] *= this[i];
                }
                this[i, i] = 1.0; // prevents rounding errors
            }

            // Gauss LR-Decomposition
            for (int k = 0; k < this.Dim(); k++)
            {
                i_max = Math.Min(this.Dim() - 1, k + this.NumLower()); // num_lower not a mistake!
                for (int i = k + 1; i <= i_max; i++)
                {
                    Debug.Assert(this[k, k] != 0.0);
                    x = -this[i, k] / this[k, k];
                    this[i, k] = -x; // assembly part of L
                    j_max = Math.Min(this.Dim() - 1, k + this.NumUpper());
                    for (int j = k + 1; j <= j_max; j++)
                    {
                        // assembly part of R
                        this[i, j] = this[i, j] + x * this[k, j];
                    }
                }
            }
        }

        /// <summary>
        /// solves Rx=y
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private List<double> RSolve(List<double> b)
        {
            Debug.Assert(this.Dim() == (int)b.Count);
            List<double> x = new List<double>(this.Dim());
            x.Resize(this.Dim());
            int j_stop;
            double sum;
            for (int i = this.Dim() - 1; i >= 0; i--)
            {
                sum = 0;
                j_stop = Math.Min(this.Dim() - 1, i + this.NumUpper());
                for (int j = i + 1; j <= j_stop; j++)
                {
                    sum += this[i, j] * x[j];
                }
                x[i] = (b[i] - sum) / this[i, i];
            }
            return x;
        }

        /// <summary>
        /// solves Ly=b
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private List<double> LSolve(List<double> b)
        {
            Debug.Assert(this.Dim() == (int)b.Count);
            List<double> x = new List<double>(this.Dim());
            x.Resize(this.Dim());
            int j_start;
            double sum;
            for (int i = 0; i < this.Dim(); i++)
            {
                sum = 0;
                j_start = Math.Max(0, i - this.NumLower());
                for (int j = j_start; j < i; j++)
                {
                    sum += this[i, j] * x[j];
                }
                x[i] = (b[i] * this[i]) - sum;
            }
            return x;
        }

        public List<double> LUSolve(List<double> b, bool is_lu_decomposed = false)
        {
            Debug.Assert(this.Dim() == (int)b.Count);

            if (is_lu_decomposed == false)
            {
                this.LUDecompose();
            }
            List<double> y = new List<double>(this.LSolve(b));
            List<double> x = new List<double>(this.RSolve(y));

            return x;
        }
    }
}
