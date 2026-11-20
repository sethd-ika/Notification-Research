using System;

namespace Notification;

public interface IBadgeService
{
    void SetCount(string target, int count);
}

public sealed class BadgeService : IBadgeService
{
    private readonly INotifyWriter<int> _writer;

    public BadgeService(INotifyWriter<int> writer)
    {
        _writer = writer;
    }

    public void SetCount(string target, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
    
        var key = new NoticeKey(target, "count");
    
        if (count == 0)
        {
            _writer.Clear(key);
            return;
        }
    
        _writer.Set(key, count);
    }
}