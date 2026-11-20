using System;
using System.Collections.Generic;

namespace Notification;

public readonly record struct NoticeKey(string Target, string Key);

public interface INotifyWriter<T>
{
    void Set(NoticeKey key, T value);
    bool Clear(NoticeKey key);
}

public interface INotifyReader<T>
{
    IReadOnlyDictionary<string, T> Read(string target);
    IDisposable Subscribe(string target, Action onChanged);
}