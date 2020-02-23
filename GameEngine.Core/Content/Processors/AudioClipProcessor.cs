using GameEngine.Core.Audio;
using GameEngine.Core.Audio.OpenAL;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Processors
{
    public class AudioClipProcessor : IContentProcessor<AudioClipRaw, AudioClip>
    {
        public AudioClip Process(AudioClipRaw contentRaw)
        {
            var buffer = new OpenALAudioBuffer(contentRaw.File);

            return new AudioClip(buffer);
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((AudioClipRaw)contentRaw);
        }
    }
}
