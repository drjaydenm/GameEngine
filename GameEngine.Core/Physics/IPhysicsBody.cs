﻿using System.Numerics;

namespace GameEngine.Core.Physics
{
    public interface IPhysicsBody
    {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Vector3 LinearVelocity { get; set; }
        Vector3 AngularVelocity { get; set; }
        float Mass { get; set; }
        float Friction { get; set; }

        void ApplyImpulse(Vector3 impulse);
    }
}
