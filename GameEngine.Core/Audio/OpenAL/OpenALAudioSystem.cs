using System;

namespace GameEngine.Core.Audio.OpenAL
{
    public class OpenALAudioSystem : IAudioSystem
    {
        private IntPtr device;

        public OpenALAudioSystem()
        {
            device = OpenAL.OpenDevice("");
            if (device == null)
            {
                throw new Exception("Could not open OpenAL device");
            }

            var context = OpenAL.CreateContext(device, null);
            OpenAL.MakeContextCurrent(context);
            OpenAL.GetError();
        }

        public INativeAudioListener CreateNativeAudioListener()
        {
            return new OpenALAudioListener();
        }

        public INativeAudioSource CreateNativeAudioSource()
        {
            return new OpenALAudioSource();
        }

        public void Dispose()
        {
            OpenAL.DestroyContext(device);
        }
    }
}
