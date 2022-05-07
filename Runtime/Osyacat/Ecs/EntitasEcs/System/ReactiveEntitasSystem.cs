using System;
using System.Collections.Generic;
using Osyacat.Ecs.Entity;
using Osyacat.Ecs.System;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal sealed class ReactiveEntitasSystem<T> : Entitas.ReactiveSystem<Entity.Entity>
    {
        private readonly IReactSystem _reactSystem;
        private readonly List<IEntity> _buffer = new();

        public ReactiveEntitasSystem(IReactSystem reactSystem, Entitas.ICollector<Entity.Entity> collector) : base(collector)
        {
            _reactSystem = reactSystem;
        }

        protected override Entitas.ICollector<Entity.Entity> GetTrigger(Entitas.IContext<Entity.Entity> context)
        {
            throw new NotSupportedException();
        }

        protected override bool Filter(Entity.Entity entity)
        {
            return true;
        }

        protected override void Execute(List<Entity.Entity> filter)
        {
            FillBuffer(filter);
            _reactSystem.React(_buffer);
            _buffer.Clear();
        }

        private void FillBuffer(List<Entity.Entity> filter)
        {
            foreach (var entity in filter)
            {
                _buffer.Add(entity);
            }
        }
    }
}