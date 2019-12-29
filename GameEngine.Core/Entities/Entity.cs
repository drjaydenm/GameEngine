using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameEngine.Core.Entities
{
    public class Entity
    {
        public string Name { get; private set; }
        public Transform Transform { get; private set; }
        public IReadOnlyCollection<IComponent> Components => components;

        private readonly Scene scene;
        private readonly ConcurrentBag<IComponent> components;

        public Entity(Scene scene, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            this.scene = scene;

            Name = name;
            components = new ConcurrentBag<IComponent>();
            Transform = new Transform(this);
        }

        public IEnumerable<T> GetComponentsOfType<T>() where T : IComponent
        {
            var matchingComponents = new List<T>();

            foreach (var component in Components)
            {
                if (component is T)
                    matchingComponents.Add((T)component);
            }

            return matchingComponents;
        }

        public void AddComponent(IComponent component)
        {
            if (component is PhysicsComponent physicsComponent)
            {
                scene.PhysicsSystem.RegisterComponent(this, physicsComponent);
            }

            components.Add(component);
            component.AttachedToEntity(this);
        }

        public void RemoveComponent(IComponent component)
        {
            component.DetachedFromEntity();
            components.TryTake(out _);

            if (component is PhysicsComponent physicsComponent)
            {
                scene.PhysicsSystem.DeregisterComponent(physicsComponent);
            }
        }

        public virtual void Update()
        {
            foreach (var component in components)
            {
                component.Update();
            }
        }
    }
}
