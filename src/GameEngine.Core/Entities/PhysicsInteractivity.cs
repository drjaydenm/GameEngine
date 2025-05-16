namespace GameEngine.Core.Entities
{
    [Flags]
    public enum PhysicsInteractivity
    {
        Dynamic = 1,
        Kinematic = 2,
        Static = 4
    }
}
