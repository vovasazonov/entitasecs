namespace Osyacat.Ecs.EntitasEcs.Component
{
    internal interface IComponentsInfo
    {
        int GetIndex<T>() where T : class;
        int Total { get; }
        string[] Names { get; }
        global::System.Type[] Types { get; }
    }
}