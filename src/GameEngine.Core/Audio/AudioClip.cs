using GameEngine.Core.Content;

namespace GameEngine.Core.Audio
{
    public class AudioClip : IContent
    {
        internal INativeAudioBuffer Buffer { get; }

        public AudioClip(INativeAudioBuffer buffer)
        {
            Buffer = buffer;
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
