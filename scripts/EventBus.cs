using Godot;
using System;
using System.Collections.Generic;

namespace project1.scripts;

/// <summary>
/// The event bus is a simple way to communicate between different parts of the code
/// without creating direct dependencies between them. It is a simple implementation
/// of the observer pattern.
/// </summary>
public partial class EventBus : Object
{
    private static Dictionary<Type, List<object>> _handlers = new (); 

    /// <summary>
    /// Subscribes a handler to a specific event type
    /// </summary>
    /// <param name="handler">Handler to be called when the event is published</param>
    /// <typeparam name="T"> The type of the event</typeparam>
    public static void Subscribe<T>(Action<T> handler)
    {
        if (!_handlers.ContainsKey(typeof(T)))
        {
            _handlers[typeof(T)] = new List<object>();
        }
        _handlers[typeof(T)].Add(handler);
    }
    
    /// <summary>
    /// Unsubscribes a handler from a specific event type
    /// </summary>
    /// <param name="handler">Handler to be removed from the event</param>
    /// <typeparam name="T"> The type of the event</typeparam>
    public static void Unsubscribe<T>(Action<T> handler)
    {
        if (_handlers.ContainsKey(typeof(T)))
        {
            _handlers[typeof(T)].Remove(handler);
        }
    }
    
    /// <summary>
    /// Publishes an event to all subscribed handlers
    /// </summary>
    /// <param name="event">The event to be published</param>
    /// <typeparam name="T"> The type of the event</typeparam>
    public static void Publish<T>(T @event)
    {
        if (_handlers.ContainsKey(typeof(T)))
        {
            foreach (var handler in _handlers[typeof(T)])
            {
                ((Action<T>)handler)(@event);
            }
        }
    }
}