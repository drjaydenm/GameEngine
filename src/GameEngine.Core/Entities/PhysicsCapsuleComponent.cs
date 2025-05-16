namespace GameEngine.Core.Entities
{
    public class PhysicsCapsuleComponent : PhysicsComponent
    {
        public float Radius { get; }
        public float Length { get; }

        public PhysicsCapsuleComponent(float radius, float length, PhysicsInteractivity interactivity) : base(interactivity)
        {
            Radius = radius;
            Length = length;
        }
    }
}
