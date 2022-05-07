using Osyacat.Ecs.System;
using Osyacat.Ecs.World;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal sealed class BeforeFrameComponentHandlerSystem<TComponent> : FrameComponentHandlerSystem<TComponent>, IBeforeUpdateSystem where TComponent : class
    {
        public BeforeFrameComponentHandlerSystem(IWorld world, bool isDestroyEntity) : base(world, isDestroyEntity)
        {
        }

        public void BeforeUpdate()
        {
            ClearFrameComponents();
        }
    }
}