using System;
using System.Collections.Generic;
using Osyacat.Ecs.Component.Event;
using Osyacat.Ecs.Entity;
using Osyacat.Ecs.Matcher;
using Osyacat.Ecs.System;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal sealed class EventComponentHandlerSystem<TComponent> : IReactSystem, IEventHandlerSystem where TComponent : class
    {
        readonly List<IComponentListener<TComponent>> _listenerBuffer;
        public Func<IEntryMatcher, IMatcher> Matcher { get; } = matcher => matcher.Has<TComponent>();

        public EventComponentHandlerSystem()
        {
            _listenerBuffer = new List<IComponentListener<TComponent>>();
        }
        
        public void React(List<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Contains<ListenerComponent<TComponent>>())
                {
                    var component = entity.Get<TComponent>();
                    _listenerBuffer.AddRange(entity.Get<ListenerComponent<TComponent>>().Value);
                    foreach (var listener in _listenerBuffer)
                    {
                        listener.OnChanged(component);
                    }
                    _listenerBuffer.Clear();
                }
            }
        }
    }
}