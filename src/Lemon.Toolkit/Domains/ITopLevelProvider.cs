using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using System;

namespace Lemon.Toolkit.Domains
{
    public interface ITopLevelProvider
    {
        WindowNotificationManager? NotificationManager {get;}
        TopLevel Ensure(TimeSpan timespan = default);
        TopLevel? Get();
        Window MainWindow { get; }
    }
}
