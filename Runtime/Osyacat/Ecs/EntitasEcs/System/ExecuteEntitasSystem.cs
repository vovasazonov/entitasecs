using Osyacat.Ecs.System;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal class ExecuteEntitasSystem<T> : Entitas.IExecuteSystem
    {
        private readonly IUpdateSystem _system;

        public ExecuteEntitasSystem(IUpdateSystem system)
        {
            _system = system;
        }

        public void Execute()
        {
            _system.Update();
        }
    }
}