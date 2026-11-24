using System;

namespace Notification;

public interface IToastService
{
    Guid Show(string message);
    bool Dismiss(Guid id);
}

public sealed class ToastService : IToastService
{
    public const string Target = "window.toast";

    private readonly INotifyWriter<ToastNotice> _writer;

    public ToastService(INotifyWriter<ToastNotice> writer)
    {
        _writer = writer;
    }

    public bool Dismiss(Guid id)
    {
        return _writer.Clear(KeyFor(id));
    }

    public Guid Show(string message)
    {
        var id = Guid.NewGuid();
        _writer.Set(KeyFor(id), new ToastNotice(id, message));
        return id;
    }

    private static NoticeKey KeyFor(Guid id) =>
        new(Target, id.ToString("N"));
}