using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Audio
{
    public class AudioListener : IComponent, IDisposable
    {
        private readonly Engine engine;
        private INativeAudioListener nativeListener;
        private Entity entity;
        private Vector3 position;

        public AudioListener(Engine engine)
        {
            this.engine = engine;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;

            if (nativeListener == null)
                nativeListener = engine.AudioSystem.CreateNativeAudioListener();
        }

        public void DetachedFromEntity()
        {
            entity = null;
        }

        public void Update()
        {
            if (entity.Transform.Position != position)
            {
                position = entity.Transform.Position;
                nativeListener.UpdatePosition(position);
            }
        }

        public void Dispose()
        {
            nativeListener.Dispose();
            nativeListener = null;
        }
    }
}
