using Silk.NET.OpenAL;

namespace GameEngine.Core.Audio.OpenAL
{
    public unsafe class OpenALAudioBuffer(AL al) : INativeAudioBuffer
    {
        internal uint BufferId { get; private set; }

        public void LoadAudioData(WaveFile file)
        {
            BufferId = al.GenBuffer();

            fixed (byte* pData = file.Data)
            {
                al.BufferData(BufferId, BufferAudioFormatToALFormat(file.Format),
                    pData, file.Data.Length, file.Frequency);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (BufferId != 0)
            {
                al.DeleteBuffer(BufferId);
            }
        }

        private static BufferFormat BufferAudioFormatToALFormat(AudioBufferFormat format)
        {
            switch (format)
            {
                case AudioBufferFormat.Mono8:
                    return BufferFormat.Mono8;
                case AudioBufferFormat.Mono16:
                    return BufferFormat.Mono16;
                case AudioBufferFormat.Stereo8:
                    return BufferFormat.Stereo8;
                case AudioBufferFormat.Stereo16:
                    return BufferFormat.Stereo16;
                default:
                    throw new InvalidOperationException("Unhandled audio format");
            }
        }
    }
}
