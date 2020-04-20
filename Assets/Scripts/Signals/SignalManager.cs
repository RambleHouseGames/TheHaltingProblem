using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SignalManager : MonoBehaviour
{
    private Dictionary<Type, Action<Signal>> listeners = new Dictionary<Type, Action<Signal>>();

    void Awake()
    {
        RegisterSingleton();
    }

    protected abstract void RegisterSingleton();

    public void AddListenner<T>(Action<Signal> callback)
    {
        if (listeners.ContainsKey(typeof(T))) // if there are already listeners for this signal type
            listeners[typeof(T)] += callback; // add this callback to the queue
        else
            listeners.Add(typeof(T), callback); // otherwise create a new listener with this callback
    }

    public void RemoveListenner<T>(Action<Signal> callback)
    {
        listeners[typeof(T)] -= callback; // remove this callback from this signal type's listener
        if (listeners[typeof(T)] == null) // if it was the only callback in the listener
            listeners.Remove(typeof(T)); //remove the listener
    }

    public void FireSignal(Signal signal)
    {
        if (listeners.ContainsKey(signal.GetType())) // if anyone is listening for this signal type
            listeners[signal.GetType()](signal); // trigger all callbacks
    }
}

public abstract class Signal {}