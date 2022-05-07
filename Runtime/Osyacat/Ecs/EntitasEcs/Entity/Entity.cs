using System.Collections.Generic;
using Osyacat.Ecs.Component.Event;
using Osyacat.Ecs.EntitasEcs.Component;
using Osyacat.Ecs.Entity;

namespace Osyacat.Ecs.EntitasEcs.Entity
{
    internal sealed class Entity : Entitas.Entity, IEntity
    {
        private readonly IComponentsInfo _info;

        public Entity(IComponentsInfo info)
        {
            _info = info;
        }

        public new void Destroy()
        {
            base.Destroy();
        }

        public T Get<T>() where T : class
        {
            int index = _info.GetIndex<T>();
            return GetByIndex<T>(index).Value;
        }

        public T Replace<T>() where T : class, new()
        {
            int index = _info.GetIndex<T>();
            var storageComponent = (ComponentShell<T>) CreateComponent(index, typeof(ComponentShell<T>));
            if (storageComponent.Value == null)
            {
                storageComponent.Value = new T();
            }
            base.ReplaceComponent(index, storageComponent);
            return storageComponent.Value;
        }

        private void ReplaceListenerComponent<T>() where T : class
        {
            if (!Contains<ListenerComponent<T>>())
            {
                var listenerComponent = Replace<ListenerComponent<T>>();
                if (listenerComponent.Value == null)
                {
                    listenerComponent.Value = new List<IComponentListener<T>>();
                }
            }
        }
        
        public void Remove<T>() where T : class
        {
            int index = _info.GetIndex<T>();
            RemoveComponent(index);
        }

        public bool Contains<T>() where T : class
        {
            int index = _info.GetIndex<T>();
            return HasComponent(index);
        }

        public void RegisterListener<T>(IComponentListener<T> listener) where T : class
        {
            ReplaceListenerComponent<T>();
            
            ListenerComponent<T> listenerComponent = Get<ListenerComponent<T>>();
            listenerComponent.Value.Add(listener);
        }

        public void UnregisterListener<T>(IComponentListener<T> listener) where T : class
        {
            ListenerComponent<T> listenerComponent = Get<ListenerComponent<T>>();
            listenerComponent.Value.Remove(listener);
        }

        private ComponentShell<T> GetByIndex<T>(int index) where T : class
        {
            return (ComponentShell<T>) base.GetComponent(index);
        }
    }
}