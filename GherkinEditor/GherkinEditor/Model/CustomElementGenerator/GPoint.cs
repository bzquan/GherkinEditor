using System;
using System.Collections.Generic;

namespace Gherkin.Model
{
    public class GPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public GPoint(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public bool IsNoise { get; set; }

        // vector operations
        public static GPoint operator +(GPoint p1, GPoint p2)
        {
            return new GPoint(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static GPoint operator -(GPoint p1, GPoint p2)
        {
            return new GPoint(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static GPoint operator *(GPoint p, double d)
        {
            return new GPoint(p.X * d, p.Y * d);
        }

        // ����(inner product, dot product, scalar product) : a�Eb �� |a||b|cos�� �� ax bx + ay by
        // zero if two vectors run orthogonally
        public double DotProduct(GPoint p)
        {
            return X * p.X + Y * p.Y;
        }

        // �O��(outer product, cross product, vector product) : a �~ b �� |a||b|sin�� �� ax by - ay bx
        // zero if two vectors run parallelly
        public double CrossProduct(GPoint p)
        {
            return X * p.Y - Y * p.X;
        }
        public static double operator *(GPoint p1, GPoint p2)
        {
            return p1.CrossProduct(p2);
        }

        // �����̂Q��
        public double SquareLength(GPoint p)
        {
            return (X - p.X) * (X - p.X) + (Y - p.Y) * (Y - p.Y);
        }

        public double Distance(GPoint p)
        {
            return (this - p).Magnitude();
        }

        public double Magnitude()
        {
            return Math.Sqrt(SquareMagnitude());
        }

        public double SquareMagnitude()
        {
            return X * X + Y * Y;
        }

        public bool IsEqual(GPoint p)
        {
            return (Math.Abs(X - p.X) < Double.Epsilon) && (Math.Abs(Y - p.Y) < Double.Epsilon);
        }
    }
}
