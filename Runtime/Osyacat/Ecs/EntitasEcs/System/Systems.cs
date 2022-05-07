using System.Collections.Generic;
using Osyacat.Ecs.System;

namespace Osyacat.Ecs.EntitasEcs.System
{
    internal class Systems : ISystems
    {
        private readonly World.World _world;
        private readonly Feature _feature = new Feature("Systems");
        private readonly List<IFixedUpdateSystem> _fixedUpdateSystems = new();
        private readonly List<IBeforeUpdateSystem> _beforeUpdateSystems = new();

        public Systems(World.World world)
        {
            _world = world;
        }

        public void Initialize()
        {
            _feature.Initialize();
        }

        public void FixedUpdate()
        {
            for (int index = 0; index < _fixedUpdateSystems.Count; index++)
            {
                _fixedUpdateSystems[index].FixedUpdate();
            }
        }

        public void BeforeUpdate()
        {
            for (int index = 0; index < _beforeUpdateSystems.Count; index++)
            {
                _beforeUpdateSystems[index].BeforeUpdate();
            }
        }

        public void Update()
        {
            _feature.Execute();
        }

        public void LateUpdate()
        {
            _feature.Cleanup();
        }

        public void Destroy()
        {
            _feature.TearDown();
        }

        public void Add<T>(T system) where T : ISystem
        {
            if (system is IReactSystem react)
            {
                var collector = CreateCollector(system, react);
                _feature.Add(new ReactiveEntitasSystem<T>(react, collector));
            }

            if (system is IUpdateSystem update)
            {
                _feature.Add(new ExecuteEntitasSystem<T>(update));
            }
            
            if (system is IInitializeSystem initialize)
            {
                _feature.Add(new InitializeEntitasSystem<T>(initialize));
            }

            if (system is ILateUpdateSystem lateUpdate)
            {
                _feature.Add(new CleanupEntitasSystem<T>(lateUpdate));
            }

            if (system is IDestroySystem destroy)
            {
                _feature.Add(new TearDownEntitasSystem<T>(destroy));
            }

            if (system is IFixedUpdateSystem fixedUpdate)
            {
                _fixedUpdateSystems.Add(fixedUpdate);
            }
            
            if (system is IBeforeUpdateSystem beforeUpdate)
            {
                _beforeUpdateSystems.Add(beforeUpdate);
            }
        }

        private Entitas.ICollector<Entity.Entity> CreateCollector<T>(T system, IReactSystem react) where T : ISystem
        {
            Entitas.ICollector<Entity.Entity> collector;
            Entitas.IMatcher<Entity.Entity> matcher = ((Matcher.Matcher)react.Matcher.Invoke(_world.Matcher)).GetMatcher();
            if (system is IEventHandlerSystem)
            {
                collector = Entitas.CollectorContextExtension.CreateCollector(_world.Context, Entitas.TriggerOnEventMatcherExtension.Added(matcher));
            }
            else
            {
                collector = _world.Context.CreateCollector(matcher);
            }

            return collector;
        }
    }
}