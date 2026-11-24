using System.Threading.Tasks;
using Avalonia.Controls;

namespace Notification;

public interface INotificationDialogService
{
    Task ShowAsync(DialogNotice notice);
}

public sealed class NotificationDialogService(Window owner) : INotificationDialogService
{
    public Task ShowAsync(DialogNotice notice)
    {
        var dialog = new NotificationDialog
        {
            DataContext = notice
        };

        return dialog.ShowDialog(owner);
    }
}