using System.Net.Mail;

namespace Notification;

public sealed class EmailValidator(IInlineNoticeService notices)
{
    private static readonly NoticeKey FormatEmail =
        new("profile.email", "format");

    public void ValidateFormat(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            notices.Dismiss(FormatEmail);
            return;
        }

        var value = email.Trim();
        bool valid = MailAddress.TryCreate(value, out var address)
            && address.Address == value
            && address.Host.Contains('.');

        if (valid)
            notices.Dismiss(FormatEmail);
        else
            notices.Show(FormatEmail, "Use an address like name@example.com.");
    }
}

public sealed class EmptyEmailValidator(IInlineNoticeService notices)
{
    private static readonly NoticeKey RequiredEmail =
        new("profile.email", "required");

    public void ValidateString(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            notices.Show(RequiredEmail, "Enter an email address.");
        else
            notices.Dismiss(RequiredEmail);
    }
}