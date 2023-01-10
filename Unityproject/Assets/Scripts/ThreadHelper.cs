using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Obsolete]
public class ThreadHelper : MonoBehaviour
{
    private List<Action> asyncQueue = new List<Action>();
    private List<Action> mainQueue = new List<Action>();
    public Queue<Action> ThreadQueue = new Queue<Action>();
    public static ThreadHelper threadHelper;
    public void AddThreadInQueue(Action action)
    {
        lock (asyncQueue)
        {
            asyncQueue.Add(action);
        }
    }
    
    private void Awake()
    {
        threadHelper = GetComponent<ThreadHelper>();
    }

    private void Update()
    {
        if (ThreadQueue.Count != 0)
        {
            Action a = ThreadQueue.Dequeue();
            a.Invoke();
        }
    }
}
