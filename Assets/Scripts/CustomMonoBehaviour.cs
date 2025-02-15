using System;
using System.Linq;
using ModestTree;
using UnityEngine;

public abstract class CustomMonoBehaviour<T> : MonoBehaviour where T : CustomMonoBehaviour<T>
{
    public event Action<T> OnInitialize;
    private bool _isInitialized;
    
    protected abstract void SetUp();

    private void Awake()
    {
        enabled = false;
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            Log.Warn(GetType().Name + " has already been initialized.");
            return;
        }

        SetUp();
        enabled = true;
        _isInitialized = true;
        OnInitialize?.Invoke((T)this);

        Log.Info(gameObject.name + " was initialized");
    }

    protected bool Subscribe<TSender>(TSender sender, Action<TSender> handler) where TSender : CustomMonoBehaviour<TSender>
    {
        return SubscribeInternal(sender, handler);
    }

    protected bool Subscribe<TSender>(Action<TSender> handler) where TSender : CustomMonoBehaviour<TSender>
    {
        var sender = GetAdjacentComponent<TSender>();
        return SubscribeInternal(sender, handler);
    }

    private bool SubscribeInternal<TSender>(TSender sender, Action<TSender> handler) where TSender : CustomMonoBehaviour<TSender>
    {
        var senderName = typeof(TSender).Name;
        if (sender == null)
        {
            Log.Error($"{gameObject.name} failed to subscribe to {senderName} (sender is null)");
            return false;
        }

        sender.OnInitialize += handler;
        return true;
    }
    
    private TSender GetAdjacentComponent<TSender>() where TSender : CustomMonoBehaviour<TSender>
    {
        var sender = GetComponent<TSender>();
        if (sender == null)
            sender = GetComponentInParent<TSender>();
        if (sender == null && transform.parent != null)
            sender = transform.parent.GetComponentsInChildren<TSender>().FirstOrDefault(s => s.transform != transform);

        if(sender == null)
            Log.Error($"{gameObject.name} failed to find adjacent component of type {typeof(TSender).Name}");
        
        return sender;
    }
}