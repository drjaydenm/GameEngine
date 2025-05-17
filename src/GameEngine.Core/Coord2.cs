using System.Numerics;

namespace GameEngine.Core
{
    public struct Coord2 : IEquatable<Coord2>
    {
        public static Coord2 One = new Coord2(1, 1);
        public static Coord2 Zero = new Coord2(0, 0);
        public static Coord2 UnitX = new Coord2(1, 0);
        public static Coord2 UnitY = new Coord2(0, 1);

        public int X;
        public int Y;

        public Coord2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"X: {X} Y: {Y}";
        }

        public override bool Equals(object obj)
        {
            return Equals((Coord2)obj);
        }

        public bool Equals(Coord2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static Vector2 Add(Coord2 val1, Vector2 val2)
        {
            return new Vector2(val1.X + val2.X, val1.Y + val2.Y);
        }

        public static Coord2 Add(Coord2 val1, Coord2 val2)
        {
            return new Coord2(val1.X + val2.X, val1.Y + val2.Y);
        }

        public static Vector2 Subtract(Coord2 val1, Vector2 val2)
        {
            return new Vector2(val1.X - val2.X, val1.Y - val2.Y);
        }

        public static Vector2 Subtract(Vector2 val1, Coord2 val2)
        {
            return new Vector2(val1.X - val2.X, val1.Y - val2.Y);
        }

        public static Coord2 Subtract(Coord2 val1, Coord2 val2)
        {
            return new Coord2(val1.X - val2.X, val1.Y - val2.Y);
        }

        public static Vector2 Multiply(Coord2 val1, Vector2 val2)
        {
            return new Vector2(val1.X * val2.X, val1.Y * val2.Y);
        }

        public static Coord2 Multiply(Coord2 val1, Coord2 val2)
        {
            return new Coord2(val1.X * val2.X, val1.Y * val2.Y);
        }

        public static bool operator ==(Coord2 val1, Coord2 val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(Coord2 val1, Coord2 val2)
        {
            return !(val1 == val2);
        }

        public static Coord2 operator -(Coord2 val1)
        {
            return new Coord2(-val1.X, -val1.Y);
        }

        public static Coord2 operator *(Coord2 val1, Coord2 val2)
        {
            return Multiply(val1, val2);
        }

        public static Coord2 operator +(Coord2 val1, Coord2 val2)
        {
            return Add(val1, val2);
        }

        public static Coord2 operator -(Coord2 val1, Coord2 val2)
        {
            return Subtract(val1, val2);
        }

        public static Vector2 operator *(Coord2 val1, Vector2 val2)
        {
            return Multiply(val1, val2);
        }

        public static Vector2 operator *(Vector2 val1, Coord2 val2)
        {
            return Multiply(val2, val1);
        }

        public static Vector2 operator +(Coord2 val1, Vector2 val2)
        {
            return Add(val1, val2);
        }

        public static Vector2 operator +(Vector2 val1, Coord2 val2)
        {
            return Add(val2, val1);
        }

        public static Vector2 operator -(Coord2 val1, Vector2 val2)
        {
            return Subtract(val1, val2);
        }

        public static Vector2 operator -(Vector2 val1, Coord2 val2)
        {
            return Subtract(val1, val2);
        }

        public static Coord2 FromNormalVector(Vector2 normal)
        {
            if (normal.X > 0 && normal.X > normal.Y)
                return UnitX;
            else if (normal.X <= 0 && normal.X < normal.Y)
                return -UnitX;
            else if (normal.Y > 0 && normal.Y > normal.X)
                return UnitY;
            else if (normal.Y <= 0 && normal.Y < normal.X)
                return -UnitY;
            else
                return UnitX;
        }
    }
}
