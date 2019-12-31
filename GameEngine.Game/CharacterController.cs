using System.Linq;
using System.Numerics;
using GameEngine.Core.Camera;
using GameEngine.Core.Entities;

namespace GameEngine.Game
{
    public class CharacterController : IComponent
    {
        private Vector3 initalPosition;
        private PhysicsCapsuleComponent capsuleComponent;
        private Entity entity;
        private ICamera camera;

        public CharacterController(Vector3 position)
        {
            initalPosition = position;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;
            capsuleComponent = new PhysicsCapsuleComponent(0.5f, 1.8f, PhysicsInteractivity.Dynamic)
            {
                Mass = 80,
                FreezeRotation = true
            };

            entity.AddComponent(capsuleComponent);
            entity.Transform.Position = initalPosition;

            camera = entity.GetComponentsOfType<ICamera>()?.First();
            camera.Position = initalPosition;
        }

        public void DetachedFromEntity()
        {
            entity.RemoveComponent(capsuleComponent);
        }

        public void Update()
        {
            camera.Position = entity.Transform.Position;
        }
    }
}
