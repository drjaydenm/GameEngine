using System;

namespace GameEngine.Core.Audio
{
    public interface IAudioSystem : IDisposable
    {
        INativeAudioListener CreateNativeAudioListener();
        INativeAudioSource CreateNativeAudioSource();
    }
}
