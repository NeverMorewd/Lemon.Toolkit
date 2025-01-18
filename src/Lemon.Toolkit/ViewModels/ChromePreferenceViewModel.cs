using Avalonia.Platform.Storage;
using Lemon.Toolkit.Domains;
using Lemon.Toolkit.Models;
using Lemon.Toolkit.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;

namespace Lemon.Toolkit.ViewModels
{
    public class ChromePreferenceViewModel : NavigationViewModelBase
    {
        public readonly ITopLevelProvider _topLevelProvider;
        public readonly IObserver<ShellParamModel> _shellService;
        public readonly FileInspectorService _fileInspectorService;
        public ChromePreferenceViewModel(ITopLevelProvider topLevelProvider,
            IObserver<ShellParamModel> shellService,
            FileInspectorService fileInspectorService,
            ILogger<ChromePreferenceViewModel> logger)
        {
            _shellService = shellService;
            _topLevelProvider = topLevelProvider;
            _fileInspectorService = fileInspectorService;

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
                .WhereNotNull()
                .Where(File.Exists)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(f => { _shellService.OnNext(new ShellParamModel { IsProcessing = true }); })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(f => (_fileInspectorService.ComputeHash(f, SHA256.Create()), _fileInspectorService.ComputeFileSize(f)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(hashes =>
                {
                    SHA256Text = hashes.Item1;
                    FileSize = $"{hashes.Item2} MB";
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
        public string? SHA256Text
        {
            get;
            set;
        }

        public ReactiveCommand<Unit, string?> BrowseFileCommand
        {
            get;
        }
    }
}
