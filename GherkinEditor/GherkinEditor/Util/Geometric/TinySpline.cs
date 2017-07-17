using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Gherkin.Util.Geometric
{
    public class TinySpline
    {
        public enum tsError
        {
            /* No error. */
            TS_SUCCESS = 0,

            /* Unable to allocate memory (using malloc/realloc). */
            TS_MALLOC = -1,

            /* The dimension of the control points are 0. */
            TS_DIM_ZERO = -2,

            /* Degree of spline (deg) >= number of control points (n_ctrlp). */
            TS_DEG_GE_NCTRLP = -3,

            /* Spline is not defined at knot value u. */
            TS_U_UNDEFINED = -4,

            /* Multiplicity of a knot (s) > order of spline.  */
            TS_MULTIPLICITY = -5,

            /* Decreasing knot vector. */
            TS_KNOTS_DECR = -6,

            /* Unexpected number of knots. */
            TS_NUM_KNOTS = -7,

            /* Spline is not derivable */
            TS_UNDERIVABLE = -8
        };

        [DllImport("TinySpline.dll", EntryPoint= "ts_fequals")]
        public static extern bool ts_fequals(double x, double y);

        [DllImport("TinySpline.dll", EntryPoint = "ts_enum_str")]
        public static extern IntPtr ts_enum_str(int err);
        public static string ts_enum_str(tsError err)
        {
            IntPtr ptr = ts_enum_str((int)err);
            // assume returned string is utf-8 encoded
            return PtrToStringUtf8(ptr);
        }

        private static string PtrToStringUtf8(IntPtr ptr) // aPtr is nul-terminated
        {
            if (ptr == IntPtr.Zero)
                return "";
            int len = 0;
            while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, len) != 0)
                len++;
            if (len == 0)
                return "";
            byte[] array = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(ptr, array, 0, len);
            return System.Text.Encoding.UTF8.GetString(array);
        }

   //     private static RationalVector ListToVector(
   //             System.Collections.Generic.IList<double> list)
   //     {
   //         if (list == null)
   //             throw new System.ArgumentNullException("List must not be null.");

   //         RationalVector vec = new RationalVector(list.Count);
   //         foreach (var val in list)
			//vec.Add(val);
   //         return vec;
   //     }
    }
}
