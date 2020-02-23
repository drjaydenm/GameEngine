using System.Numerics;

namespace GameEngine.Core.Audio.OpenAL
{
    public class OpenALAudioSource : INativeAudioSource
    {
        public AudioClip AudioClip
        {
            get => audioClip;
            set
            {
                audioClip = value;

                var openALBuffer = (OpenALAudioBuffer)AudioClip.Buffer;
                OpenAL.SourceQueueBuffer(sourceId, openALBuffer.BufferId);
            }
        }

        private int sourceId;
        private AudioClip audioClip;

        public OpenALAudioSource()
        {
            var sources = new int[1];
            OpenAL.GenSources(sources);
            sourceId = sources[0];
        }

        public void Play()
        {
            OpenAL.SourcePlay(sourceId);
        }

        public void Stop()
        {
            OpenAL.SourceStop(sourceId);
        }

        public void Dispose()
        {
            OpenAL.DeleteSource(sourceId);
        }

        public void UpdatePosition(Vector3 position)
        {
            OpenAL.Source(sourceId, OpenAL.ALSource3f.Position, position.X, position.Y, position.Z);
        }
    }
}
