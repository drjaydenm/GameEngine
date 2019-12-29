namespace GameEngine.Core.Entities
{
    public interface IComponent
    {
        void AttachedToEntity(Entity entity);
        void DetachedFromEntity();
        void Update();
    }
}
