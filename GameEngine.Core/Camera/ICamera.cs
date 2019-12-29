using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Camera
{
    public interface ICamera : IComponent
    {
        Matrix4x4 View { get; }
        Matrix4x4 Projection { get; }
        Vector3 Position { get; set; }
        Vector3 ViewDirection { get; set; }
        Quaternion Rotation { get; }
        float NearClip { get; }
        float FarClip { get; }
        float FieldOfView { get; }
        float AspectRatio { get; }
    }
}
