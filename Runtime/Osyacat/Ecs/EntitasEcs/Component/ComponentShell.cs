namespace Osyacat.Ecs.EntitasEcs.Component
{
    internal sealed class ComponentShell<T> : Entitas.IComponent where T : class
    {
        public T Value;
    }
}