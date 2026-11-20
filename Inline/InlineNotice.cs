namespace Notification;

public sealed record InlineNotice(string Message);

public interface IInlineNoticeService
{
    void Show(NoticeKey key, string message);
    void Dismiss(NoticeKey key);
}

public sealed class InlineNoticeService : IInlineNoticeService
{
    private readonly KeyedNoticeStore<InlineNotice> _store;

    public InlineNoticeService(KeyedNoticeStore<InlineNotice> store)
    {
        _store = store;
    }
    // Inside InlineNoticeService
    public void Show(NoticeKey key, string message)
    {
        _store.Set(key, new InlineNotice(message));
    }

    public void Dismiss(NoticeKey key)
    {
        _store.Clear(key);
    }
}