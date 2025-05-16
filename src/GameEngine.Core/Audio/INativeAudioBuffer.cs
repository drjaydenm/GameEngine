namespace GameEngine.Core.Audio;

public interface INativeAudioBuffer : IDisposable
{
    public void LoadAudioData(WaveFile file);
}
