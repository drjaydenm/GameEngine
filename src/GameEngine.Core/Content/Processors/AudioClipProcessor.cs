using GameEngine.Core.Audio;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Processors
{
    public class AudioClipProcessor : IContentProcessor<AudioClipRaw, AudioClip>
    {
        private readonly Engine _engine;

        public AudioClipProcessor(Engine engine)
        {
            this._engine = engine;
        }

        public AudioClip Process(AudioClipRaw contentRaw)
        {
            var buffer = _engine.AudioSystem.CreateNativeAudioBuffer();
            buffer.LoadAudioData(contentRaw.File);

            return new AudioClip(buffer);
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((AudioClipRaw)contentRaw);
        }
    }
}
