using System.Numerics;

namespace GameEngine.Core;

public struct Color : IEquatable<Color>
{
    public static readonly Color Black = new Color(0f, 0f, 0f, 1f);
    public static readonly Color Blue = new Color(0f, 0f, 1f, 1f);
    public static readonly Color Brown = new Color(0.6f, 0.4f, 0.2f, 1f);
    public static readonly Color CornflowerBlue = new Color(0.392f, 0.584f, 0.929f, 1f);
    public static readonly Color Cyan = new Color(0f, 1f, 1f, 1f);
    public static readonly Color DarkGray = new Color(0.25f, 0.25f, 0.25f, 1f);
    public static readonly Color Gray = new Color(0.5f, 0.5f, 0.5f, 1f);
    public static readonly Color Green = new Color(0f, 1f, 0f, 1f);
    public static readonly Color LightGray = new Color(0.75f, 0.75f, 0.75f, 1f);
    public static readonly Color Magenta = new Color(1f, 0f, 1f, 1f);
    public static readonly Color Orange = new Color(1f, 0.65f, 0f, 1f);
    public static readonly Color Pink = new Color(1f, 0.75f, 0.8f, 1f);
    public static readonly Color Purple = new Color(0.5f, 0f, 0.5f, 1f);
    public static readonly Color Red = new Color(1f, 0f, 0f, 1f);
    public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);
    public static readonly Color White = new Color(1f, 1f, 1f, 1f);
    public static readonly Color Yellow = new Color(1f, 1f, 0f, 1f);

    private readonly Vector4 _channels;

    public float R => _channels.X;
    public float G => _channels.Y;
    public float B => _channels.Z;
    public float A => _channels.W;

    public Color(float r, float g, float b, float a)
    {
        _channels = new Vector4(r, g, b, a);
    }

    public Color(Vector4 channels)
    {
        _channels = channels;
    }

    public override string ToString()
    {
        return $"R: {R} G: {G} B: {B} A: {A}";
    }

    public Vector4 ToVector4()
    {
        return _channels;
    }

    public bool Equals(Color other)
    {
        return _channels.Equals(other._channels);
    }

    public override bool Equals(object obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _channels.GetHashCode();
    }

    public static bool operator ==(Color val1, Color val2)
    {
        return val1.Equals(val2);
    }

    public static bool operator !=(Color val1, Color val2)
    {
        return !(val1 == val2);
    }
}
