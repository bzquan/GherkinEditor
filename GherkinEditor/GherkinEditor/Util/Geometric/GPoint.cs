using System;
using System.Collections.Generic;

namespace Gherkin.Util.Geometric
{
    public class GPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double W { get; set; } = 1.0; // Weight

        public GPoint(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public GPoint(double x, double y, double weight)
        {
            X = x;
            Y = y;
            W = weight;
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

        public static GPoint operator *(double d, GPoint p)
        {
            return p * d;
        }

        // “àÏ(inner product, dot product, scalar product) : aEb  |a||b|cosƒÆ  ax bx + ay by
        // zero if two vectors run orthogonally
        public double DotProduct(GPoint p)
        {
            return X * p.X + Y * p.Y;
        }

        // ŠOÏ(outer product, cross product, vector product) : a ~ b  |a||b|sinƒÆ  ax by - ay bx
        // zero if two vectors run parallelly
        public double CrossProduct(GPoint p)
        {
            return X * p.Y - Y * p.X;
        }
        public static double operator *(GPoint p1, GPoint p2)
        {
            return p1.CrossProduct(p2);
        }

        // ‹——£‚Ì‚Qæ
        public double SquareDistance(GPoint p)
        {
            double dx = X - p.X;
            double dy = Y - p.Y;
            return dx * dx + dy * dy;
        }

        // square distance from a point to a segment
        public double SquareDistance2Segment(GPoint p1, GPoint p2)
        {
            var x = p1.X;
            var y = p1.Y;
            var dx = p2.X - x;
            var dy = p2.Y - y;

            if (!dx.Equals(0.0) || !dy.Equals(0.0))
            {
                var t = ((X - x) * dx + (Y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = X - x;
            dy = Y - y;

            return (dx * dx) + (dy * dy);
        }

        public double Distance(GPoint p)
        {
            return (this - p).Length();
        }

        public double Length()
        {
            return Math.Sqrt(SquareLength());
        }

        public double SquareLength()
        {
            return X * X + Y * Y;
        }

        public bool IsEqual(GPoint p)
        {
            return (Math.Abs(X - p.X) < Double.Epsilon) && (Math.Abs(Y - p.Y) < Double.Epsilon);
        }

        public bool Zero => IsZero(X) && IsZero(Y);

        public static bool IsZero(double v) => Math.Abs(v) < double.Epsilon;
    }
}
