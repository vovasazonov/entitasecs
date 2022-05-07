using System;
using Osyacat.Ecs.EntitasEcs.Component;
using Osyacat.Ecs.EntitasEcs.Matcher;
using Osyacat.Ecs.Entity;
using Osyacat.Ecs.Matcher;
using Osyacat.Ecs.World;

namespace Osyacat.Ecs.EntitasEcs.World
{
    internal sealed class World : IWorld
    {
        private readonly EntryMatcher _matcher;
        private readonly Entitas.Context<Entity.Entity> _context;

        internal EntryMatcher Matcher => _matcher;
        internal Entitas.Context<Entity.Entity> Context => _context;

        public World(IComponentsInfo componentsInfo, string name = "World")
        {
            _context = CreateContext();
            _matcher = new EntryMatcher(componentsInfo);

            Entitas.Context<Entity.Entity> CreateContext()
            {
                Entitas.ContextInfo contextInfo = new Entitas.ContextInfo(name, componentsInfo.Names, componentsInfo.Types);

                Entitas.IAERC AercFactory(Entitas.IEntity entity)
                {
#if (ENTITAS_FAST_AND_UNSAFE)
                    return new Entitas.UnsafeAERC();
#else
                    return new Entitas.SafeAERC(entity);
#endif
                }

                Entity.Entity EntityFactory() => new Entity.Entity(componentsInfo);

                return new Entitas.Context<Entity.Entity>(componentsInfo.Total, 0, contextInfo, AercFactory, EntityFactory);
            }
        }

        IEntity IWorld.CreateEntity()
        {
            return _context.CreateEntity();
        }

        private IFilter GetFilter(IMatcher matcher)
        {
            Entitas.IMatcher<Entity.Entity> entitasMatcher = ((Matcher.Matcher) matcher).GetMatcher();
            Entitas.IGroup<Entity.Entity> group = _context.GetGroup(entitasMatcher);
            IFilter filter = new Filter(group);
            return filter;
        }

        public IFilter GetFilter(Func<IEntryMatcher, IMatcher> matcher)
        {
            return GetFilter(matcher.Invoke(_matcher));
        }
    }
}