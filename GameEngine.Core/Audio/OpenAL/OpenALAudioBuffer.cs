namespace GameEngine.Core.Audio.OpenAL
{
    public class OpenALAudioBuffer : IAudioBuffer
    {
        internal int BufferId { get; }

        public OpenALAudioBuffer(WaveFile file)
        {
            OpenAL.GenBuffers(1, out int bufferId);
            BufferId = bufferId;

            OpenAL.BufferData(BufferId, BufferAudioFormatToALFormat(file.Format),
                file.Data, file.Data.Length, file.Frequency);
        }

        public void Dispose()
        {
            OpenAL.DeleteBuffers(BufferId);
        }

        private OpenAL.ALFormat BufferAudioFormatToALFormat(AudioBufferFormat format)
        {
            switch (format)
            {
                case AudioBufferFormat.Mono8:
                    return OpenAL.ALFormat.Mono8;
                case AudioBufferFormat.Mono16:
                    return OpenAL.ALFormat.Mono16;
                case AudioBufferFormat.Stereo8:
                    return OpenAL.ALFormat.Stereo8;
                case AudioBufferFormat.Stereo16:
                    return OpenAL.ALFormat.Stereo16;
                default:
                    throw new System.Exception("Unhandled audio format");
            }
        }
    }
}
