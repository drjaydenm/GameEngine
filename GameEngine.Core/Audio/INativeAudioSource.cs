using System;
using System.Numerics;

namespace GameEngine.Core.Audio
{
    public interface INativeAudioSource : IDisposable
    {
        AudioClip AudioClip { get; set; }

        void Play();
        void Stop();
        void UpdatePosition(Vector3 position);
    }
}
