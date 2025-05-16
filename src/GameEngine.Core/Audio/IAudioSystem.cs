namespace GameEngine.Core.Audio
{
    public interface IAudioSystem : IDisposable
    {
        INativeAudioBuffer CreateNativeAudioBuffer();
        INativeAudioListener CreateNativeAudioListener();
        INativeAudioSource CreateNativeAudioSource();
    }
}
