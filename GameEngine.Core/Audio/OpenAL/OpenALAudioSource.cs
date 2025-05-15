using System.Numerics;
using Silk.NET.OpenAL;

namespace GameEngine.Core.Audio.OpenAL
{
    public class OpenALAudioSource(AL al) : INativeAudioSource
    {
        public AudioClip AudioClip
        {
            get => _audioClip;
            set
            {
                _audioClip = value;

                var openALBuffer = (OpenALAudioBuffer)AudioClip.Buffer;
                al.SourceQueueBuffers(_sourceId, [openALBuffer.BufferId]);
            }
        }
        public float Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                al.SetSourceProperty(_sourceId, SourceFloat.Gain, value);
            }
        }

        private readonly uint _sourceId = al.GenSource();
        private AudioClip _audioClip;
        private float _gain;

        public void Play()
        {
            al.SourcePlay(_sourceId);
        }

        public void Stop()
        {
            al.SourceStop(_sourceId);
        }

        public void UpdatePosition(Vector3 position)
        {
            al.SetSourceProperty(_sourceId, SourceVector3.Position, position.X, position.Y, position.Z);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            al.DeleteSource(_sourceId);
        }
    }
}
