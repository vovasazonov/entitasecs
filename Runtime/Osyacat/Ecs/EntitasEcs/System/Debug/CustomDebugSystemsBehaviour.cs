using UnityEngine;

namespace Osyacat.Ecs.EntitasEcs.System.Debug
{
    public class CustomDebugSystemsBehaviour : MonoBehaviour
    {
        private CustomDebugSystems _systems;

        public CustomDebugSystems systems => this._systems;

        public void Init(CustomDebugSystems systems) => this._systems = systems;
    }
}