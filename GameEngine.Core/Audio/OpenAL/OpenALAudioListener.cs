using System.Numerics;

namespace GameEngine.Core.Audio.OpenAL
{
    public class OpenALAudioListener : INativeAudioListener
    {
        public void UpdatePosition(Vector3 position)
        {
            OpenAL.Listener(OpenAL.ALListener3f.Position, position.X, position.Y, position.Z);
        }

        public void Dispose()
        {
        }
    }
}
