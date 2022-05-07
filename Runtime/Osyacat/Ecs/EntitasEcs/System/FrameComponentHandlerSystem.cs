using System.Collections.Generic;
using Osyacat.Ecs.Entity;
using Osyacat.Ecs.World;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal abstract class FrameComponentHandlerSystem<TComponent> where TComponent : class
    {
        private readonly List<IEntity> _buffer = new();
        private readonly bool _isDestroyEntity;
        private readonly IFilter _filter;

        protected FrameComponentHandlerSystem(IWorld world, bool isDestroyEntity)
        {
            _isDestroyEntity = isDestroyEntity;
            _filter = world.GetFilter(m => m.Has<TComponent>());
        }

        protected void ClearFrameComponents()
        {
            _filter.GetEntities(_buffer);

            foreach (var entity in _buffer)
            {
                if (_isDestroyEntity)
                {
                    entity.Destroy();
                }
                else
                {
                    entity.Remove<TComponent>();
                }
            }
        }
    }
}