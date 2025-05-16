using System.Numerics;
using Silk.NET.OpenAL;

namespace GameEngine.Core.Audio.OpenAL
{
    public class OpenALAudioListener(AL al) : INativeAudioListener
    {
        public void UpdatePosition(Vector3 position)
        {
            al.SetListenerProperty(ListenerVector3.Position, position.X, position.Y, position.Z);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
