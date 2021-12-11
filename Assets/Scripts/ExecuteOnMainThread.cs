using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class ExecuteOnMainThread : MonoBehaviour {
    
    private static readonly ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();

    void Update()
    {
  //      Debug.Log("Update");
        lock (Queue)
        {
            if(!Queue.IsEmpty)
            {
                if (Queue.TryDequeue(out var action))
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }

    public static void RunOnMainThread(Action action)
    {
        lock (Queue)
        {
            Queue.Enqueue(action);            
        }
    }
}