using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central event system for loose coupling between game systems.
/// Allows systems to publish and subscribe to events without direct dependencies.
/// </summary>
public class EventSystem
{
    private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    /// <summary>
    /// Subscribe to an event with a typed callback
    /// </summary>
    public static void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = null;
        }
        eventTable[eventName] = (Action<T>)eventTable[eventName] + listener;
    }

    /// <summary>
    /// Subscribe to an event with no parameters
    /// </summary>
    public static void Subscribe(string eventName, Action listener)
    {
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = null;
        }
        eventTable[eventName] = (Action)eventTable[eventName] + listener;
    }

    /// <summary>
    /// Unsubscribe from an event with typed callback
    /// </summary>
    public static void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action<T>)eventTable[eventName] - listener;
        }
    }

    /// <summary>
    /// Unsubscribe from an event with no parameters
    /// </summary>
    public static void Unsubscribe(string eventName, Action listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action)eventTable[eventName] - listener;
        }
    }

    /// <summary>
    /// Publish an event with data
    /// </summary>
    public static void Publish<T>(string eventName, T data)
    {
        if (eventTable.ContainsKey(eventName) && eventTable[eventName] != null)
        {
            try
            {
                ((Action<T>)eventTable[eventName]).Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking event {eventName}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Publish an event with no parameters
    /// </summary>
    public static void Publish(string eventName)
    {
        if (eventTable.ContainsKey(eventName) && eventTable[eventName] != null)
        {
            try
            {
                ((Action)eventTable[eventName]).Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking event {eventName}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Clear all event subscriptions
    /// </summary>
    public static void Clear()
    {
        eventTable.Clear();
    }

    /// <summary>
    /// Clear subscriptions for a specific event
    /// </summary>
    public static void Clear(string eventName)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable.Remove(eventName);
        }
    }
}
