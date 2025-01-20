using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Lemon.Toolkit.Domains;
using Lemon.Toolkit.Shells;
using System;
using System.Threading;

namespace Lemon.Toolkit.Services
{
    public class TopLevelProvider : ITopLevelProvider
    {
        private TopLevel? _topLevel;
        private WindowNotificationManager? _notificationManager;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private Window? _mainWindow;

        public TopLevelProvider()
        {
        }
        public WindowNotificationManager? NotificationManager
        {
            get
            {
                if (_notificationManager == null)
                {
                    Ensure();
                    if (_topLevel != null)
                    {
                        _notificationManager = new WindowNotificationManager(_topLevel)
                        {
                            MaxItems = 3,
                            Position = NotificationPosition.BottomRight
                        };
                    }
                }
                return _notificationManager;
            }
        }
        public Window MainWindow
        {
            get
            {
                Ensure();
                return _mainWindow!;
            }
        }
        public TopLevel? Get()
        {
            return GetTopLevelCore();
        }
        public TopLevel Ensure(TimeSpan timespan = default)
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    _semaphore.Wait();
                    var topLevel = GetTopLevelCore();
                    if (topLevel != null)
                    {
                        return topLevel;
                    }

                    if (timespan == default)
                    {
                        throw new InvalidOperationException($"Fail to get TopLevel!");
                    }
                    if (DateTime.UtcNow - startTime > timespan)
                    {
                        throw new TimeoutException($"Fail to get TopLevel within {timespan.TotalSeconds} seconds");
                    }

                    Thread.Sleep(10);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
        private TopLevel? GetTopLevelCore()
        {
            if (_topLevel != null)
            {
                return _topLevel;
            }

            if (Avalonia.Application.Current is not null)
            {
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    if (desktop.MainWindow is not null)
                    {
                        _mainWindow = desktop.MainWindow;
                        _topLevel = TopLevel.GetTopLevel(_mainWindow);
                        return _topLevel;
                    }
                }
            }
            return null;
        }
    }
}