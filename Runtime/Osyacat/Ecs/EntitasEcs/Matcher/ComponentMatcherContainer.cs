using System.Collections.Generic;
using Osyacat.Ecs.EntitasEcs.Component;

namespace Osyacat.Ecs.EntitasEcs.Matcher
{
    internal sealed class ComponentMatcherContainer
    {
        private readonly IComponentsInfo _info;
        private readonly Dictionary<int, Entitas.Matcher<Entity.Entity>> _matchers = new();

        public ComponentMatcherContainer(IComponentsInfo info)
        {
            _info = info;
        }

        public Entitas.IMatcher<Entity.Entity> GetMatcher<T>() where T : class
        {
            int index = _info.GetIndex<T>();

            if (!_matchers.TryGetValue(index, out var matcher))
            {
                matcher = (Entitas.Matcher<Entity.Entity>) Entitas.Matcher<Entity.Entity>.AllOf(index);
                matcher.componentNames = _info.Names;
                _matchers.Add(index, matcher);
            }

            return matcher;
        }
    }
}