﻿using System.Collections.Concurrent;
using System.Reflection;

namespace MHEventBus;

public class EventBus(string Name)
{
    private List<Type> StaticSubscribers { get; } = new();
    private List<object> SubscriberClasses { get; } = new();
    private List<Type> EventTypes { get; }= new();
    private ConcurrentDictionary<Type, ConcurrentBag<MethodInfo>> Handlers { get; } = new();
    private bool HasStarted { get; set; }
    
    public bool EnableInheritance { get; set; } = false;
    
    
    public void Register(object subscriber)
    {
        SubscriberClasses.Add(subscriber);
        if (!HasStarted)
        {
            return;
        }
        var subscribers = subscriber.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(SubscribeEvent), false).Any());
        foreach (var methodInfo in subscribers)
        {
            var @params = methodInfo.GetParameters();
            if (@params.Length == 1)
            {
                var parameterType = @params[0].ParameterType;
                if (parameterType.IsAssignableTo(typeof(IEvent)) && EventTypes.Contains(parameterType))
                {
                    lock (Handlers)
                    {
                        if (Handlers.ContainsKey(parameterType))
                        {
                            if (!Handlers[parameterType].Contains(methodInfo))
                            {
                                Handlers[parameterType].Add(methodInfo);
                            }
                        }
                        else
                        {
                            Handlers.TryAdd(parameterType, new ConcurrentBag<MethodInfo>());
                            Handlers[parameterType].Add(methodInfo);
                        }
                    }
                }
            }
        }
    }

    public void RegisterMany(IEnumerable<object> subscriberS)
    {
        SubscriberClasses.AddRange(subscriberS);
        if (!HasStarted)
        {
            return;
        }
        foreach (var subscriber in subscriberS)
        {
            var subscribers = subscriber.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(SubscribeEvent), false).Any());
            foreach (var methodInfo in subscribers)
            {
                var @params = methodInfo.GetParameters();
                if (@params.Length == 1)
                {
                    var parameterType = @params[0].ParameterType;
                    if (parameterType.IsAssignableTo(typeof(IEvent)) && EventTypes.Contains(parameterType))
                    {
                        lock (Handlers)
                        {
                            if (Handlers.ContainsKey(parameterType))
                            {
                                if (!Handlers[parameterType].Contains(methodInfo))
                                {
                                    Handlers[parameterType].Add(methodInfo);
                                }
                            }
                            else
                            {
                                Handlers.TryAdd(parameterType, new ConcurrentBag<MethodInfo>());
                                Handlers[parameterType].Add(methodInfo);
                            }
                        }
                    }
                }
            }
        }
    }

    public bool Unregister(object subscriber)
    {
        return SubscriberClasses.Remove(subscriber);
    }

    private void InvokeHandlers(IEvent @event, ConcurrentBag<MethodInfo> handlers)
    {
        foreach (var methodInfo in handlers)
        {
            var owner = SubscriberClasses.Any(s => s.GetType() == methodInfo.DeclaringType)
                ? SubscriberClasses.Where(s => s.GetType() == methodInfo.DeclaringType)
                : new List<object>();
            var staticOwners = StaticSubscribers.Any(s => s == methodInfo.DeclaringType) 
                ? StaticSubscribers.Where(s => s == methodInfo.DeclaringType) 
                : new List<Type>();
                        
            if (methodInfo.IsStatic)
            {
                foreach (var staticOwner in staticOwners) 
                {
                    methodInfo.Invoke(staticOwner, [@event]);
                }
            }
            else
            {
                foreach (var o in owner) 
                { 
                    methodInfo.Invoke(o, [@event]);
                }
            }
        }
    }

    public void PushEvent(IEvent @event)
    {
        if (HasStarted)
        {
            if (EnableInheritance)
            {
                if (EventTypes.Any(ev => ev.IsInstanceOfType(@event)))
                {
                    lock (Handlers)
                    {
                        foreach (Type type in EventTypes.Where(ev => ev.IsInstanceOfType(@event)))
                        {
                            if (Handlers.TryGetValue(type, out ConcurrentBag<MethodInfo> handlers))
                            {
                                InvokeHandlers(@event, handlers);
                            }
                        }
                    }
                }
            }
            else
            {
                if (EventTypes.Contains(@event.GetType()))
                {
                    lock (Handlers)
                    {
                        if (Handlers.TryGetValue(@event.GetType(), out ConcurrentBag<MethodInfo> handlers))
                        {
                            InvokeHandlers(@event, handlers);
                        }
                    }
                }
            }
        }
    }

    public async Task PushEventAsync(IEvent @event)
    {
        await Task.Run(() =>
        {
            PushEvent(@event);
        });
    }

    private void ProcessRegistrants()
    {
        Handlers.Clear();
        Parallel.ForEach(SubscriberClasses, subscriber =>
        {
            var subscribers = subscriber.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(SubscribeEvent), false).Any());
            foreach (var methodInfo in subscribers)
            {
                var @params = methodInfo.GetParameters();
                if (@params.Length == 1)
                {
                    var parameterType = @params[0].ParameterType;
                    if (parameterType.IsAssignableTo(typeof(IEvent)))
                    {
                        lock (Handlers)
                        {
                            if (Handlers.ContainsKey(parameterType))
                            {
                                if (!Handlers[parameterType].Contains(methodInfo))
                                {
                                    Handlers[parameterType].Add(methodInfo);
                                }
                            }
                            else
                            {
                                Handlers.TryAdd(parameterType, new ConcurrentBag<MethodInfo>());
                                Handlers[parameterType].Add(methodInfo);
                            }
                        }
                    }
                }
            }
        });
        
        Parallel.ForEach(StaticSubscribers, subscriber =>
        {
            var subscribers = subscriber.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(SubscribeEvent), false).Any());
            foreach (var methodInfo in subscribers)
            {
                var @params = methodInfo.GetParameters();
                if (@params.Length == 1)
                {
                    var parameterType = @params[0].ParameterType;
                    if (parameterType.IsAssignableTo(typeof(IEvent)))
                    {
                        lock (Handlers)
                        {
                            if (Handlers.ContainsKey(parameterType))
                            {
                                if (!Handlers[parameterType].Contains(methodInfo))
                                {
                                    Handlers[parameterType].Add(methodInfo);
                                }
                            }
                            else
                            {
                                Handlers.TryAdd(parameterType, new ConcurrentBag<MethodInfo>());
                                Handlers[parameterType].Add(methodInfo);
                            }
                        }
                    }
                }
            }
        });
    }

    private void DiscoverStaticSubscribers()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> types = new();
        foreach (var assembly in assemblies)
        {
            types.AddRange(assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(EventBusSubscriber), false).Any() && !type.IsAbstract && !type.IsInterface));
        }
        types.RemoveAll(type => ((EventBusSubscriber)type.GetCustomAttributes(typeof(EventBusSubscriber), false)[0]).EventBusName != Name);
        StaticSubscribers.Clear();
        StaticSubscribers.AddRange(types);
    }

    private void RegisterEvent(Type eventType)
    {
        EventTypes.Add(eventType);
    }

    private void RegisterEvents(IEnumerable<Type> eventTypes)
    {
        EventTypes.AddRange(eventTypes);
    }

    public void StartUp()
    {
        HasStarted = true;
        DiscoverStaticSubscribers();
        RegisterEvent(typeof(RegisterEventTypesEvent));
        ProcessRegistrants();
        PushEvent(new RegisterEventTypesEvent(this));
    }

    public void ShutDown()
    {
        HasStarted = false;
        SubscriberClasses.Clear();
        StaticSubscribers.Clear();
        Handlers.Clear();
        EventTypes.Clear();
    }
    
    public class RegisterEventTypesEvent(EventBus Bus) : IEvent
    {

        public void RegisterEvent(Type ev) 
        {
            if (!ev.IsAssignableTo(typeof(IEvent)))
            {
                throw new ArgumentException($"Unable to register an event {ev.FullName} that is not an event");
            }
            
            Bus.RegisterEvent(ev);
        }

        public void RegisterEvents(IEnumerable<Type> events)
        {
            var eventTypes = events as Type[] ?? events.ToArray();
            foreach (var @event in eventTypes)
            {
                if (!@event.IsAssignableTo(typeof(IEvent)))
                {
                    throw new ArgumentException($"Unable to register an event {@event.FullName} that is not an event");
                }
            }
            
            Bus.RegisterEvents(eventTypes);
        }
        
        public void Dispose()
        {
            //nothing
        }
    }
}