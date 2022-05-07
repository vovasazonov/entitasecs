using System;
using System.Collections.Generic;
using System.Linq;

namespace Osyacat.Ecs.EntitasEcs.Component
{
    internal sealed class ComponentsInfo : IComponentsInfo
    {
        private readonly Dictionary<Type, int> _componentByIndex;
        
        public ComponentsInfo(IEnumerable<Type> components)
        {
            Types = components.ToArray();
            Total = Types.Length;
            Names = Types.Select(type =>
            {
                Type[] generic = type.GetGenericArguments();
                return generic[0].Name;
            }).ToArray();
            _componentByIndex = new Dictionary<Type, int>(Total);
            int index = 0;
            foreach (var type in Types)
            {
                _componentByIndex.Add(type, index);
                ++index;
            }
        }

        public int GetIndex<T>() where T : class => _componentByIndex[typeof(ComponentShell<T>)];
        public int Total { get; }
        public string[] Names { get; }
        public Type[] Types { get; }
    }
}