using System;

namespace Notification;

public sealed record ToastNotice(Guid Id, string Message);