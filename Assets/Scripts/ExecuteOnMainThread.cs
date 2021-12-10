using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class ExecuteOnMainThread : MonoBehaviour {
    
    private static readonly ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();

    void Update()
    {
        if(!Queue.IsEmpty)
        {
            while(Queue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }

    public static void RunOnMainThread(Action action)
    {
        Queue.Enqueue(action);
    }
}