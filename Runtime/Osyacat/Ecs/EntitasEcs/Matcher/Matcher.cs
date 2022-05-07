using System.Collections.Generic;
using System.Linq;
using Osyacat.Ecs.Matcher;

namespace Osyacat.Ecs.EntitasEcs.Matcher
{
    internal sealed class Matcher : IMatcher
    {
        private readonly ComponentMatcherContainer _container;
        private readonly HashSet<Entitas.IMatcher<Entity.Entity>> _has = new HashSet<Entitas.IMatcher<Entity.Entity>>();
        private readonly HashSet<Entitas.IMatcher<Entity.Entity>> _none = new HashSet<Entitas.IMatcher<Entity.Entity>>();

        public Matcher(ComponentMatcherContainer container)
        {
            _container = container;
        }
        
        internal Entitas.IMatcher<Entity.Entity> GetMatcher()
        {
            var matcher = Entitas.Matcher<Entity.Entity>.AllOf(_has.ToArray());

            if (_none.Any())
            {
                matcher.NoneOf(_none.ToArray());
            }

            return matcher;
        }

        public IMatcher Has<T>() where T : class
        {
            var matcher = _container.GetMatcher<T>();
            _has.Add(matcher);
            return this;
        }

        public IMatcher None<T>() where T : class
        {
            var matcher = _container.GetMatcher<T>();
            _none.Add(matcher);
            return this;
        }
    }
}