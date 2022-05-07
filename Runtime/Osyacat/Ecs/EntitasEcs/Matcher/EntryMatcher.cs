using Osyacat.Ecs.EntitasEcs.Component;
using Osyacat.Ecs.Matcher;

namespace Osyacat.Ecs.EntitasEcs.Matcher
{
    internal sealed class EntryMatcher : IEntryMatcher
    {
        private readonly ComponentMatcherContainer _container;

        public EntryMatcher(IComponentsInfo info)
        {
            _container = new ComponentMatcherContainer(info);
        }

        public IMatcher Has<T>() where T : class
        {
            var matcher = new Matcher(_container);
            return matcher.Has<T>();
        }
    }
}