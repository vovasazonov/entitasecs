using System;
using System.Collections.Generic;
using System.Linq;
using Osyacat.Ecs.Component.Event;
using Osyacat.Ecs.Component.Frame;
using Osyacat.Ecs.EntitasEcs.Component;
using Osyacat.Ecs.EntitasEcs.System;
using Osyacat.Ecs.System;
using Osyacat.Ecs.World;

namespace Osyacat.Ecs.EntitasEcs
{
    public class EntitasEcsUniverse : IEcsUniverse
    {
        public IWorld World { get; }
        public ISystems Systems { get; }

        public EntitasEcsUniverse(Type[] components)
        {
            object[][] componentsAttributes = GetComponentsAttributes(components);
            Type[] worldComponents = GetWorldComponents(components, componentsAttributes);
            ComponentsInfo componentsInfo = new ComponentsInfo(worldComponents);
            World.World world = new World.World(componentsInfo);
            World = world;
            Systems = new Systems(world);
            InitializeAttributeComponentsHandlerSystems(components, componentsAttributes);
        }

        private object[][] GetComponentsAttributes(Type[] components)
        {
            object[][] attributes = new object[components.Length][];

            for (int i = 0; i < components.Length; i++)
            {
                attributes[i] = components[i].GetCustomAttributes(false);
            }

            return attributes;
        }

        private Type[] GetWorldComponents(Type[] components, object[][] componentsAttributes)
        {
            Type[] shellComponents = GetShellComponents(components);
            Type[] shellListenerComponents = GetShellListenerComponents(components, componentsAttributes);

            return shellComponents.Concat(shellListenerComponents).ToArray();
        }

        private Type[] GetShellComponents(Type[] components)
        {
            Type[] shellComponents = new Type[components.Length];
            Type componentShell = typeof(ComponentShell<>);

            for (int i = 0; i < components.Length; i++)
            {
                Type component = components[i];
                Type shelledComponent = componentShell.MakeGenericType(component);
                shellComponents[i] = shelledComponent;
            }

            return shellComponents;
        }

        private Type[] GetShellListenerComponents(Type[] components, object[][] componentsAttributes)
        {
            List<Type> shellComponents = new List<Type>();
            Type listenerComponentType = typeof(ListenerComponent<>);
            Type componentShell = typeof(ComponentShell<>);

            for (int i = 0; i < components.Length; i++)
            {
                Type component = components[i];
                object[] attributes = componentsAttributes[i];

                foreach (var attribute in attributes)
                {
                    if (attribute is EventComponentAttribute)
                    {
                        Type shelledListenerComponent = listenerComponentType.MakeGenericType(component);
                        Type shelledComponent = componentShell.MakeGenericType(shelledListenerComponent);
                        shellComponents.Add(shelledComponent);
                    }
                }
            }

            return shellComponents.ToArray();
        }

        private void InitializeAttributeComponentsHandlerSystems(Type[] components, object[][] componentsAttributes)
        {
            for (int i = 0; i < components.Length; i++)
            {
                Type component = components[i];
                object[] attributes = componentsAttributes[i];

                foreach (var attribute in attributes)
                {
                    ISystem system = null;
                    
                    switch (attribute)
                    {
                        case FrameComponentAttribute frameAttribute:
                            if (frameAttribute.IsClearBeforeUpdate)
                            {
                                Type frameHandlerSystemType = typeof(BeforeFrameComponentHandlerSystem<>).MakeGenericType(component);
                                system = (ISystem)Activator.CreateInstance(frameHandlerSystemType, World, frameAttribute.IsDestroyEntity);
                            }
                            else
                            {
                                Type frameHandlerSystemType = typeof(LateFrameComponentHandlerSystem<>).MakeGenericType(component);
                                system = (ISystem)Activator.CreateInstance(frameHandlerSystemType, World, frameAttribute.IsDestroyEntity);
                            }
                            break;
                        case EventComponentAttribute:
                            Type eventHandlerSystemType = typeof(EventComponentHandlerSystem<>).MakeGenericType(component);
                            system = (ISystem)Activator.CreateInstance(eventHandlerSystemType);
                            break;
                    }

                    if (system != null)
                    {
                        Systems.Add(system);
                    }
                }
            }
        }
    }
}