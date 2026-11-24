using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Notification;

public partial class NotificationDialog : Window
{
    public NotificationDialog() => InitializeComponent();

    private void CloseDialog(object? sender, RoutedEventArgs e) => Close();
}