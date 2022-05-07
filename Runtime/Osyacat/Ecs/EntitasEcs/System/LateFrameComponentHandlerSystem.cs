using Osyacat.Ecs.System;
using Osyacat.Ecs.World;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal sealed class LateFrameComponentHandlerSystem<TComponent> : FrameComponentHandlerSystem<TComponent>, ILateUpdateSystem where TComponent : class
    {
        public LateFrameComponentHandlerSystem(IWorld world, bool isDestroyEntity) : base(world, isDestroyEntity)
        {
        }

        public void LateUpdate()
        {
            ClearFrameComponents();
        }
    }
}