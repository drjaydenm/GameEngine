using System;
using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Audio
{
    public class AudioSource : IComponent, IDisposable
    {
        public AudioClip AudioClip
        {
            get => audioClip;
            set
            {
                audioClip = value;
                if (nativeSource != null)
                    nativeSource.AudioClip = value;
            }
        }
        private AudioClip audioClip;

        private readonly Engine engine;
        private INativeAudioSource nativeSource;
        private Entity entity;
        private Vector3 position;

        public AudioSource(Engine engine)
        {
            this.engine = engine;
        }

        public void Play()
        {
            nativeSource.Play();
        }

        public void Stop()
        {
            nativeSource.Stop();
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;

            if (nativeSource == null)
                nativeSource = engine.AudioSystem.CreateNativeAudioSource();

            nativeSource.AudioClip = audioClip;
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
                nativeSource.UpdatePosition(position);
            }
        }

        public void Dispose()
        {
            nativeSource.Dispose();
            nativeSource = null;
        }
    }
}
