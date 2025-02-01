using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;
using Lemon.Toolkit.Models;
using Lemon.Toolkit.Services;
using Lemon.Toolkit.Services.OrmServices;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Xml.Linq;
using Lemon.Toolkit.Extensions;
using Lemon.Toolkit.Domains;
using ReactiveUI.Fody.Helpers;

namespace Lemon.Toolkit.ViewModels;

[SupportedOSPlatform("windows")]
public class TestViewModel : NavigationViewModelBase
{
    private static readonly TextBlock _text = new() { Text="I am static!" };
    private readonly ILogger _logger;
    private readonly ITopLevelProvider _topLevelProvider;
    private readonly WindowsFeatureService _windowsFeatureService;
    private readonly EnvironmentVariableService _environmentVariableService;
    public TestViewModel(WindowsFeatureService windowsFeatureService,
        EnvironmentVariableService environmentVariableService,
        ITopLevelProvider topLevelProvider,
        ILogger<TestViewModel> logger)
    {
        TestCommand = ReactiveCommand.CreateFromTask(TestCommandAsync);
        AddPathCommand = ReactiveCommand.CreateFromTask<string?>(AddToPathAsync);
        _windowsFeatureService = windowsFeatureService;
        _topLevelProvider = topLevelProvider;
        _environmentVariableService = environmentVariableService;
        _logger = logger;

        BrowseCommand = ReactiveCommand.CreateFromTask<Unit, string?>(BrowseFileAync);
        BrowseCommand.Subscribe(f => 
        {
            SourceFilePath = f;
        });
        ParseCommand = ReactiveCommand.CreateFromTask<string?>(f => 
        {
            return Task.Run(() => 
            {
                if (!string.IsNullOrEmpty(f))
                {
                    var data = ParseXml(f);
                    SaveToPostgres(data);
                }
            });
        });
    }

    private async Task<string?> BrowseFileAync(Unit unit)
    {
        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            FileTypeFilter = [AvaloniauiExtension.FileTypeXml]
        };
        var files = await _topLevelProvider.Ensure().StorageProvider.OpenFilePickerAsync(options);
        if (files != null && files.Any())
        {
            return files[0].TryGetLocalPath();
        }
        return null;
    }

    private async Task AddToPathAsync(string? arg)
    {
        await Task.Yield();
        if (!string.IsNullOrEmpty(arg))
        {
            _environmentVariableService.WritePath(arg);
        }
    }

    private async Task TestCommandAsync()
    {
        await Task.Run(() => 
        {
            try
            {
                var can = _windowsFeatureService.CanRegisterTask();
                _logger.LogDebug($"CanRegisterTask:{can}");
                if (can)
                {
                    _windowsFeatureService.CreateTask($"Lemon.Test", Environment.ProcessPath!, "Lemon", 1);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "TestCommandAsync");
                if (ex.InnerException is not null)
                {
                    _logger.LogError(ex.InnerException, "TestCommandAsync");
                }
            }
        });
    }

    public IDataTemplate TestNewTemplate
        => new FuncDataTemplate<string>((x, __) =>
        {
            return new TextBlock { Text = $"test:{x}" };
        });

    public IDataTemplate TestTemplate
        => new FuncDataTemplate<string>((_, __) =>
        {
            return _text;
        });
    [Reactive]
    public string? SourceFilePath
    {
        get;
        set;
    }

    public ReactiveCommand<Unit, Unit> TestCommand { get; }
    public ReactiveCommand<string?, Unit> AddPathCommand { get; }
    public ReactiveCommand<Unit, string?> BrowseCommand { get; }
    public ReactiveCommand<string?, Unit> ParseCommand { get; }

    public List<SourceWordModel> ParseXml(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        var items = new List<SourceWordModel>();

        foreach (var element in doc.Descendants("CustomizeListItem"))
        {
            var item = new SourceWordModel
            {
                Word = element.Attribute("word")?.Value,
                ItemType = int.Parse(element.Attribute("itemType")?.Value ?? "-9999"),
                AddTime = DateTime.ParseExact(
                    element.Attribute("addTimeP")?.Value,
                    "yyyyMMddTHHmmss",
                    CultureInfo.InvariantCulture).ToUniversalTime(),
                Rating = int.Parse(element.Attribute("rating")?.Value ?? "0"),
                CategoryTag = element.Attribute("categoryTag")?.Value
            };
            items.Add(item);
        }

        return items;
    }
    public void SaveToPostgres(List<SourceWordModel> items)
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated(); 
        context.SourceVocabulary.AddRange(items);
        context.SaveChanges();
    }
}