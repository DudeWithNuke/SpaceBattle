using System;
using System.Linq;
using ModestTree;
using UnityEngine;

public abstract class InitializeMonoBehaviour<T> : MonoBehaviour where T : InitializeMonoBehaviour<T>
{
    public event Action<T> OnInitialize;

    private readonly string _name = typeof(T).Name;

    private void Awake()
    { 
        enabled = false;
    }

    public void Initialize()
    {
        SetUp();
        enabled = true;
        OnInitialize?.Invoke((T)this);
        Log.Info(_name + " was initialized");
    }

    protected void Subscribe<TSender>(Action<TSender> handler) where TSender : InitializeMonoBehaviour<TSender>
    {
        var senderName = typeof(TSender).Name;
        var sender = GetAdjacentComponent<TSender>();
        if (sender != null)
        {
            sender.OnInitialize += handler;
            Log.Info(_name + " subscribed to " + senderName);
        }
        else
        {
            Log.Error(_name + " failed to subscribe to " + senderName);
        }
    }

    private TSender GetAdjacentComponent<TSender>() where TSender : InitializeMonoBehaviour<TSender>
    {
        var sender = GetComponent<TSender>();
        if (sender == null)
            sender = GetComponentInParent<TSender>();
        if (sender == null && transform.parent != null)
            sender = transform.parent.GetComponentsInChildren<TSender>().FirstOrDefault(s => s.transform != transform);
        return sender;
    }
    
    protected abstract void SetUp();
}