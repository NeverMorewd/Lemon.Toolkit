using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Lemon.Toolkit.Domains;
using Lemon.Toolkit.Models;
using Lemon.Toolkit.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace Lemon.Toolkit.ViewModels
{
    [RequiresUnreferencedCode("")]
    public class FileInspectorViewModel: NavigationViewModelBase
    {
        private readonly CompositeDisposable? _disposables;
        private readonly ITopLevelProvider _topLevelProvider;
        private readonly IObserver<ShellParamModel> _shellService;
        private readonly FileInspectorService _fileInspectorService;
        private readonly ILogger _logger;
        public FileInspectorViewModel(ITopLevelProvider topLevelProvider, 
            IObserver<ShellParamModel> shellService,
            FileInspectorService fileInspectorService,
            ILogger<FileInspectorViewModel> logger) 
        {
            _logger = logger;
            _topLevelProvider = topLevelProvider;
            _shellService = shellService;
            _fileInspectorService = fileInspectorService;
            CopyCommand = ReactiveCommand.CreateFromTask<object>(async (obj) =>
            {
                string? content = null;
                if (obj is string text)
                {
                    content = text;
                }
                else if (obj is IEnumerable objects)
                {
                    var texts = objects.Cast<string>();
                    content = string.Join('-', texts);
                }
                if (string.IsNullOrEmpty(content) || content == "-")
                {
                    _logger.LogError("Can not copy empty string");
                    _topLevelProvider.NotificationManager!.Show(new Notification("Error", "Can not copy empty string!", NotificationType.Error));
                }
                else
                {
                    await _topLevelProvider.Ensure().Clipboard!.SetTextAsync(content);
                    _topLevelProvider.NotificationManager!.Show(new Notification("Success", "Copied", NotificationType.Success));
                }
            });
            BrowseFileCommand = ReactiveCommand.CreateFromTask<Unit, string?>(async _ =>
            {
                FilePickerOpenOptions options = new()
                {
                    AllowMultiple = false
                };
                var files = await _topLevelProvider.Ensure().StorageProvider.OpenFilePickerAsync(options);
                if (files != null && files.Any())
                {
                    return files[0].TryGetLocalPath();
                }
                return null;
            });
            BrowseFileCommand
                .Do(f => FilePath = f)
                //.ObserveOn(RxApp.TaskpoolScheduler)
                .WhereNotNull()
                .Where(File.Exists)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(f => { _shellService.OnNext(new ShellParamModel { IsProcessing = true }); })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(f => (_fileInspectorService.ComputeHash(f, MD5.Create()), _fileInspectorService.ComputeHash(f, SHA256.Create()), _fileInspectorService.ComputeFileSize(f)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(hashes =>
                {
                    MD5Text = hashes.Item1;
                    SHA256Text = hashes.Item2;
                    FileSize = $"{hashes.Item3} MB";
                    _shellService.OnNext(new ShellParamModel { IsProcessing = false });
                });
        }

        [Reactive]
        public string? FilePath
        {
            get;
            set;
        }
        [Reactive]
        public string? FileSize
        {
            get;
            set;
        }

        [Reactive]
        public string? MD5Text
        {
            get;
            set;
        }
        [Reactive]
        public string? SHA256Text
        {
            get;
            set;
        }

        public ReactiveCommand<Unit, string?> BrowseFileCommand
        {
            get;
        }
        public ReactiveCommand<object, Unit> CopyCommand
        {
            get;
        }
    }
}
