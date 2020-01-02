using System;
using System.Numerics;

namespace GameEngine.Core
{
    public struct Coord3 : IEquatable<Coord3>
    {
        public static Coord3 One = new Coord3(1, 1, 1);
        public static Coord3 Zero = new Coord3(0, 0, 0);
        public static Coord3 UnitX = new Coord3(1, 0, 0);
        public static Coord3 UnitY = new Coord3(0, 1, 0);
        public static Coord3 UnitZ = new Coord3(0, 0, 1);

        public int X;
        public int Y;
        public int Z;

        public Coord3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"X: {X} Y: {Y} Z: {Z}";
        }

        public override bool Equals(object obj)
        {
            return Equals((Coord3)obj);
        }

        public bool Equals(Coord3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }

        public static Vector3 Add(Coord3 val1, Vector3 val2)
        {
            return new Vector3(val1.X + val2.X, val1.Y + val2.Y, val1.Z + val2.Z);
        }

        public static Coord3 Add(Coord3 val1, Coord3 val2)
        {
            return new Coord3(val1.X + val2.X, val1.Y + val2.Y, val1.Z + val2.Z);
        }

        public static Vector3 Multiply(Coord3 val1, Vector3 val2)
        {
            return new Vector3(val1.X * val2.X, val1.Y * val2.Y, val1.Z * val2.Z);
        }

        public static Coord3 Multiply(Coord3 val1, Coord3 val2)
        {
            return new Coord3(val1.X * val2.X, val1.Y * val2.Y, val1.Z * val2.Z);
        }

        public static bool operator ==(Coord3 val1, Coord3 val2)
        {
            if ((object)val1 == null)
                return (object)val2 == null;

            return val1.Equals(val2);
        }

        public static bool operator !=(Coord3 val1, Coord3 val2)
        {
            return !(val1 == val2);
        }

        public static Coord3 operator -(Coord3 val1)
        {
            return new Coord3(-val1.X, -val1.Y, -val1.Z);
        }

        public static Coord3 operator *(Coord3 val1, Coord3 val2)
        {
            return Multiply(val1, val2);
        }

        public static Coord3 operator +(Coord3 val1, Coord3 val2)
        {
            return Add(val1, val2);
        }

        public static Vector3 operator *(Coord3 val1, Vector3 val2)
        {
            return Multiply(val1, val2);
        }

        public static Vector3 operator *(Vector3 val1, Coord3 val2)
        {
            return Multiply(val2, val1);
        }

        public static Vector3 operator +(Coord3 val1, Vector3 val2)
        {
            return Add(val1, val2);
        }

        public static Vector3 operator +(Vector3 val1, Coord3 val2)
        {
            return Add(val2, val1);
        }
    }
}
