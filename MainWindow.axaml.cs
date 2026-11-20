using System;
using System.Threading;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Notification;

public partial class MainWindow : Window
{
    private const string InboxTarget = "inbox";
    private const string ConnectionId = "connection";
    private const string EmailTarget = "profile.email";
    private readonly KeyedNoticeStore<BannerNotice> _store = new();
    private readonly IBannerService _banners;
    private readonly IDisposable _subscription;
    private readonly IDisposable _inlineSubscription;
    private int _unreadCount;
    private readonly KeyedNoticeStore<int> _badgeStore = new();
    private readonly IBadgeService _badges;
    private readonly KeyedNoticeStore<InlineNotice> _inlineStore = new();
    private readonly EmailValidator _emailValidator;
    private readonly EmptyEmailValidator _emptyEmailValidator;
    private CancellationTokenSource? _showBannerToken;

    public MainWindow()
    {
        InitializeComponent();

        _banners = new BannerService(_store);
        _subscription = _store.Subscribe(BannerService.Target, RefreshBanners);
        RefreshBanners();

        _badges = new BadgeService(_badgeStore);
        var badgeSubscription = _badgeStore.Subscribe(InboxTarget, RefreshInboxBadge);
        RefreshInboxBadge();

        _emailValidator = new EmailValidator(new InlineNoticeService(_inlineStore));
        _emptyEmailValidator = new EmptyEmailValidator(new InlineNoticeService(_inlineStore));

        _inlineSubscription = _inlineStore.Subscribe(EmailTarget, RefreshInlineNotices);
        RefreshInlineNotices();

        var typingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        
        void ValidateEmailNow()
        {
            typingTimer.Stop();
            var email = EmailInput.Text;
            _emptyEmailValidator.ValidateString(email);
            _emailValidator.ValidateFormat(email);
        }
        
        typingTimer.Tick += (_, _) => ValidateEmailNow();
        
        EmailInput.TextChanged += (_, _) =>
        {
            typingTimer.Stop();
            typingTimer.Start();
        };
        
        EmailInput.LostFocus += (_, _) => ValidateEmailNow();

        Closed += (_, _) =>
        {
            typingTimer.Stop();
            _subscription.Dispose();
            _showBannerToken?.Cancel();
            _inlineSubscription.Dispose();
            badgeSubscription.Dispose();
        };
    }

    private void RefreshInlineNotices()
    {
        EmailNotices.ItemsSource = _inlineStore.Read(EmailTarget).Values;
    }

    private void RefreshInboxBadge()
    {
        var entries = _badgeStore.Read(InboxTarget);
        bool hasCount = entries.TryGetValue("count", out int count);

        InboxBadge.IsVisible = hasCount;
        InboxBadgeText.Text = hasCount ? count.ToString() : string.Empty;
    }

    private void RefreshBanners()
    {
        var entries = _store.Read(BannerService.Target);
        BannerList.ItemsSource = entries.Values;

        bool hasConnectionBanner = entries.ContainsKey(ConnectionId);
        UpdateButton.IsEnabled = hasConnectionBanner;
        DismissButton.IsEnabled = hasConnectionBanner;
    }

    private async void ShowBanner(object? sender, RoutedEventArgs e)
    {
        _showBannerToken?.Cancel();

        using var source = new CancellationTokenSource();
        _showBannerToken = source;

        try
        {
            _banners.Show(ConnectionId, "You're offline.");
            await Task.Delay(TimeSpan.FromSeconds(2), source.Token);

            // An update may have canceled this timer just before the UI resumed.
            source.Token.ThrowIfCancellationRequested();
            _banners.Dismiss(ConnectionId);
        }
        catch (OperationCanceledException) when (source.IsCancellationRequested)
        {
            // Another action took over the banner. Its current state should stay.
        }
        finally
        {
            if (ReferenceEquals(_showBannerToken, source))
                _showBannerToken = null;
        }
    }

    private void UpdateBanner(object? sender, RoutedEventArgs e)
    {
        _showBannerToken?.Cancel();
        _banners.Show(ConnectionId, "Reconnecting…");
    }

    private void DismissBanner(object? sender, RoutedEventArgs e)
    {
        _showBannerToken?.Cancel();
        _banners.Dismiss(ConnectionId);
    }

    private void ReceiveMessage(object? sender, RoutedEventArgs e)
    {
        _unreadCount++;
        _badges.SetCount(InboxTarget, _unreadCount);
    }

    private void MarkAllRead(object? sender, RoutedEventArgs e)
    {
        _unreadCount = 0;
        _badges.SetCount(InboxTarget, _unreadCount);
    }
}