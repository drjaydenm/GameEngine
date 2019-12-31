using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Game
{
    public class CharacterController : IComponent
    {
        private Vector3 initalPosition;
        private PhysicsCapsuleComponent capsuleComponent;
        private Entity entity;

        public CharacterController(Vector3 position)
        {
            initalPosition = position;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;
            capsuleComponent = new PhysicsCapsuleComponent(0.5f, 1.8f, PhysicsInteractivity.Dynamic);

            entity.AddComponent(capsuleComponent);
            entity.Transform.Position = initalPosition;
        }

        public void DetachedFromEntity()
        {
            entity.RemoveComponent(capsuleComponent);
        }

        public void Update()
        {
            
        }
    }
}
