using GameEngine.Core.Audio;

namespace GameEngine.Core.Content.Raw
{
    public class AudioClipRaw : IContentRaw
    {
        public WaveFile File { get; }

        public AudioClipRaw(WaveFile file)
        {
            File = file;
        }
    }
}
