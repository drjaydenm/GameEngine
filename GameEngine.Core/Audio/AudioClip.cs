using GameEngine.Core.Content;

namespace GameEngine.Core.Audio
{
    public class AudioClip : IContent
    {
        internal IAudioBuffer Buffer { get; }

        public AudioClip(IAudioBuffer buffer)
        {
            Buffer = buffer;
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
