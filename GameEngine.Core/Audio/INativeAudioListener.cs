using System;
using System.Numerics;

namespace GameEngine.Core.Audio
{
    public interface INativeAudioListener : IDisposable
    {
        void UpdatePosition(Vector3 position);
    }
}
