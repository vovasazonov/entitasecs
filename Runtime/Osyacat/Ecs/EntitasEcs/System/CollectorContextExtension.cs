namespace Osyacat.Ecs.EntitasEcs.System
{
    internal static class CollectorContextExtension
    {
        public static Entitas.ICollector<TEntity> CreateCollector<TEntity>(
            this Entitas.IContext<TEntity> context,
            Entitas.IMatcher<TEntity> matcher)
            where TEntity : class, Entitas.IEntity
        {
            return context.CreateCollector(new Entitas.TriggerOnEvent<TEntity>(matcher, Entitas.GroupEvent.Added));
        }

        public static Entitas.ICollector<TEntity> CreateCollector<TEntity>(
            this Entitas.IContext<TEntity> context,
            params Entitas.TriggerOnEvent<TEntity>[] triggers)
            where TEntity : class, Entitas.IEntity
        {
            Entitas.IGroup<TEntity>[] groups = new Entitas.IGroup<TEntity>[triggers.Length];
            Entitas.GroupEvent[] groupEvents = new Entitas.GroupEvent[triggers.Length];
            for (int index = 0; index < triggers.Length; ++index)
            {
                groups[index] = context.GetGroup(triggers[index].matcher);
                groupEvents[index] = triggers[index].groupEvent;
            }

            return new Entitas.Collector<TEntity>(groups, groupEvents);
        }
    }
}