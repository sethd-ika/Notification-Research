namespace Notification;

public interface IBannerService
{
    void Show(string id, string message);
    bool Dismiss(string id);
}

public sealed class BannerService : IBannerService
{
    public const string Target = "window.banner";
    private readonly INotifyWriter<BannerNotice> _writer;

    public BannerService(INotifyWriter<BannerNotice> writer)
    {
        _writer = writer;
    }

    public void Show(string id, string message)
    {
        var key = new NoticeKey(Target, id);
        var notice = new BannerNotice(message);
        _writer.Set(key, notice);
    }

    public bool Dismiss(string id)
    {
        var key = new NoticeKey(Target, id);
        return _writer.Clear(key);
    }
}