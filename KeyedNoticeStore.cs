using System;
using System.Collections.Generic;

namespace Notification;

public sealed class KeyedNoticeStore<T> : INotifyWriter<T>, INotifyReader<T>
{
    private readonly Dictionary<string, Dictionary<string, T>> _entriesByTarget = new();
    private readonly Dictionary<string, Dictionary<Guid, Action>> _subscriptionsByTarget = new();

    public bool Clear(NoticeKey key)
    {
        if (!_entriesByTarget.TryGetValue(key.Target, out var entries))
            return false;
    
        if (!entries.Remove(key.Key))
            return false;
    
        if (entries.Count == 0)
            _entriesByTarget.Remove(key.Target);
    
        NotifyChanged(key.Target);
        return true;
    }

    public void Set(NoticeKey key, T value)
    {
        if (!_entriesByTarget.TryGetValue(key.Target, out var entries))
        {
            entries = new Dictionary<string, T>();
            _entriesByTarget[key.Target] = entries;
        }

        entries[key.Key] = value;
        NotifyChanged(key.Target);
    }

    public IReadOnlyDictionary<string, T> Read(string target)
    {
        if (!_entriesByTarget.TryGetValue(target, out var entries))
            return new Dictionary<string, T>();

        return new Dictionary<string, T>(entries);
    }

    public IDisposable Subscribe(string target, Action onChanged)
    {
        ArgumentNullException.ThrowIfNull(onChanged);

        if (!_subscriptionsByTarget.TryGetValue(target, out var callbacks))
        {
            callbacks = new Dictionary<Guid, Action>();
            _subscriptionsByTarget[target] = callbacks;
        }

        var id = Guid.NewGuid();
        callbacks.Add(id, onChanged);

        return new Subscription(() =>
        {
            callbacks.Remove(id);

            if (callbacks.Count == 0)
                _subscriptionsByTarget.Remove(target);
        });
    }

    private void NotifyChanged(string target)
    {
        if (!_subscriptionsByTarget.TryGetValue(target, out var callbacks))
            return;

        // A callback can unsubscribe itself while notifications are being delivered.
        var snapshot = new List<Action>(callbacks.Values);

        foreach (var callback in snapshot)
            callback();
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        public void Dispose()
        {
            var cleanup = _unsubscribe;
            _unsubscribe = null;
            cleanup?.Invoke();
        }
    }
} 