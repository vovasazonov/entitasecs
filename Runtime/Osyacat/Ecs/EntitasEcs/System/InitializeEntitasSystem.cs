using Osyacat.Ecs.System;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal class InitializeEntitasSystem<T> : Entitas.IInitializeSystem
    {
        private readonly IInitializeSystem _system;

        public InitializeEntitasSystem(IInitializeSystem system)
        {
            _system = system;
        }

        public void Initialize()
        {
            _system.Initialize();
        }
    }
}