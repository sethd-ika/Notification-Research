using Notification;
using Xunit;

namespace Notification.Tests;

public class KeyedNoticeStoreTests
{
    [Fact]
    public void Subscribe_StopsNotifyingAfterDispose()
    {
        var store = new KeyedNoticeStore<string>();
        var key = new NoticeKey("profile.email", "format");
        var calls = 0;

        using (store.Subscribe(key.Target, () => calls++))
        {
            Assert.Equal(0, calls);

            store.Set(key, "Invalid email");
            Assert.Equal(1, calls);
        }

        store.Set(key, "Missing @");
        Assert.Equal(1, calls);
    }
}