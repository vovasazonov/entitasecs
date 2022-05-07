using Osyacat.Ecs.EntitasEcs.System.Debug;

namespace Osyacat.Ecs.EntitasEcs.System
{
#if (!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)

    internal class Feature : CustomDebugSystems
    {
        public Feature(string name) : base(name)
        {
        }
    }
    
#else
    internal class Feature : Entitas.Systems
    {
        public Feature(string name)
        {
        }
    }

#endif
}