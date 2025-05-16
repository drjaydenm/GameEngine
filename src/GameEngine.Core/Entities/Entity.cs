namespace GameEngine.Core.Entities
{
    public class Entity
    {
        public string Name { get; private set; }
        public Transform Transform { get; private set; }
        public IReadOnlyList<IComponent> Components => components;
        public Scene Scene { get; }

        private readonly List<IComponent> components;

        public Entity(Scene scene, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            Scene = scene;

            Name = name;
            components = new List<IComponent>();
            Transform = new Transform(this);
        }

        public IEnumerable<T> GetComponentsOfType<T>() where T : IComponent
        {
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    yield return (T)components[i];
            }
        }

        public void AddComponent(IComponent component)
        {
            component.AttachedToEntity(this);
            components.Add(component);

            if (component is PhysicsComponent physicsComponent)
            {
                Scene.PhysicsSystem.RegisterComponent(physicsComponent);
            }
        }

        public void RemoveComponent(IComponent component)
        {
            components.Remove(component);
            component.DetachedFromEntity();

            if (component is PhysicsComponent physicsComponent)
            {
                Scene.PhysicsSystem.DeregisterComponent(physicsComponent);
            }
        }

        public virtual void Update()
        {
            for (var i = 0; i < components.Count; i++)
            {
                components[i].Update();
            }
        }
    }
}
