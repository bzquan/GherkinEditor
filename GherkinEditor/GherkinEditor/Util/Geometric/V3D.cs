using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util.Geometric
{
    /// <summary>
    /// Represents 3D vector structure.
    /// </summary>
    public struct V3D
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public double X;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public double Y;

        /// <summary>
        /// Z coordinate;
        /// </summary>
        public double Z;

        public V3D(double x = 0.0, double y = 0.0, double z = 0.0) { X = x; Y = y; Z = z; }

        /// <summary>
        /// Test if both vectors are equal.
        /// </summary>
        /// <param name="obj">The object the equality is tested with.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            V3D v3d = (V3D)obj;
            return IsEqual(X, v3d.X) && IsEqual(Y, v3d.Y) && IsEqual(Z, v3d.Z);
        }

        /// <summary>
        /// Returns the hash code calculated for this instance.
        /// </summary>
        /// <returns>Integer most likely to be unique for different vectors.</returns>
        public override int GetHashCode() => 17 * (17 * X.GetHashCode() + Y.GetHashCode()) + Z.GetHashCode();

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// Gets the value indicating whether this vector is a zero vector (a vector whose all coordinates equal zero).
        /// </summary>
        public bool Zero => IsZero(X) && IsZero(Y) && IsZero(Z);

        /// <summary>
        /// Binary equality test.
        /// </summary>
        /// <param name="a">Left side.</param>
        /// <param name="b">Right side.</param>
        /// <returns>True if both vectors are not null and equal.</returns>
        public static bool operator ==(V3D a, V3D b) => a.Equals(b);

        /// <summary>
        /// Binary inequality test.
        /// </summary>
        /// <param name="a">Left side.</param>
        /// <param name="b">Right side.</param>
        /// <returns>True if one of the vectors is null or the vectors are not equals.</returns>
        public static bool operator !=(V3D a, V3D b) => !a.Equals(b);

        /// <summary>
        /// Vector addition.
        /// </summary>
        /// <param name="a">Left side.</param>
        /// <param name="b">Right side.</param>
        /// <returns>The sum of vectors.</returns>
        public static V3D operator +(V3D a, V3D b) => new V3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        /// <summary>
        /// Vector subtraction.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The difference of vectors.</returns>
        public static V3D operator -(V3D a, V3D b) => new V3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        /// <summary>
        /// Vector negation.
        /// </summary>
        /// <param name="a">Unary argument.</param>
        /// <returns>The negative vector.</returns>
        public static V3D operator -(V3D a) => new V3D(-a.X, -a.Y, -a.Z);

        /// <summary>
        /// Unary plus. Clones the vector.
        /// </summary>
        /// <param name="a">Unary argument.</param>
        /// <returns>This vector clone.</returns>
        public static V3D operator +(V3D a) => new V3D(+a.X, +a.Y, +a.Z);

        /// <summary>
        /// Scalar product.
        /// </summary>
        /// <param name="a">Left side vector.</param>
        /// <param name="k">Right side scalar.</param>
        /// <returns>The scalar product.</returns>
        public static V3D operator *(V3D a, double k) => new V3D(k * a.X, k * a.Y, k * a.Z);

        /// <summary>
        /// Scalar product.
        /// </summary>
        /// <param name="k">Left side scalar.</param>
        /// <param name="a">Right side vector.</param>
        /// <returns>The scalar product.</returns>
        public static V3D operator *(double k, V3D a) => new V3D(k * a.X, k * a.Y, k * a.Z);

        /// <summary>
        /// Scalar quotient.
        /// </summary>
        /// <param name="a">Left side vector.</param>
        /// <param name="k">Right side scalar.</param>
        /// <returns>The scalar quotient.</returns>
        public static V3D operator /(V3D a, double k) => new V3D(a.X / k, a.Y / k, a.Z / k);

        /// <summary>
        /// Scalar quotient.
        /// </summary>
        /// <param name="k">Left side scalar.</param>
        /// <param name="a">Right side vector.</param>
        /// <returns>The scalar quotient.</returns>
        public static V3D operator /(double k, V3D a) => new V3D(k / a.X, k / a.Y, k / a.Z);

        /// <summary>
        /// Returns dot product.
        /// </summary>
        /// <param name="a">A vector.</param>
        /// <returns>Dot product.</returns>
        public double Dot(V3D a) => X * a.X + Y * a.Y + Z * a.Z;

        /// <summary>
        /// Returns cross product.
        /// </summary>
        /// <param name="a">A vector.</param>
        /// <returns>Cross product.</returns>
        public V3D Cross(V3D a) => new V3D(Y * a.Z - Z * a.Y, Z * a.X - X * a.Z, X * a.Y - Y * a.X);

        /// <summary>
        /// Returns the vector between a and b that divides vector (a - b) in t ratio. For zero it's a, for 1 it's b.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        /// <param name="t">A number between 0 and 1.</param>
        /// <returns>the vector between a and b that divides vector (a - b) in t ratio. For zero it's a, for 1 it's b.</returns>
        public static V3D Interpolate(V3D a, V3D b, double t) => new V3D(a.X * (1.0 - t) + b.X * t, a.Y * (1.0 - t) + b.Y * t, a.Z * (1.0 - t) + b.Z * t);


        public static bool IsEqual(double v1, double v2) => Math.Abs(v1 - v2) < double.Epsilon;
        public static bool IsZero(double v) => Math.Abs(v) < double.Epsilon;
    }
}
