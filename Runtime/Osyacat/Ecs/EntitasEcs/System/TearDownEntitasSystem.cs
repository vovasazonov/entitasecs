using Osyacat.Ecs.System;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal class TearDownEntitasSystem<T> : Entitas.ITearDownSystem
    {
        private readonly IDestroySystem _system;

        public TearDownEntitasSystem(IDestroySystem system)
        {
            _system = system;
        }

        public void TearDown()
        {
            _system.Destroy();
        }
    }
}