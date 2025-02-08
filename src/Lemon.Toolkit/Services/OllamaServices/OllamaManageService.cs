using Lemon.HandyLib.Toolkits;
using Lemon.Toolkit.Models.Ollama.Platforms;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Services.OllamaServices;

public class OllamaManageService
{
    private readonly ILogger _logger;
    private readonly IOllamaApi _ollamaApi;
    private Process? _process;
    private readonly SemaphoreSlim _semaphore;
    private readonly IMetaDataProvider _metaDataProvider;
    public OllamaManageService(IOllamaApi ollamaApi,
        IMetaDataProvider metaDataProvider,
        ILogger<OllamaManageService> logger)
    {
        _logger = logger;
        _ollamaApi = ollamaApi;
        _semaphore = new SemaphoreSlim(1, 1);
        _metaDataProvider = metaDataProvider;
    }
    public Process? Process
    {
        get
        {
            if (_process == null || _process.HasExited)
            {
                _process?.Dispose();
                _process = LinkProcess(_metaDataProvider.ProcessName_Server);
                if (_process != null)
                {
                    _process.EnableRaisingEvents = true;
                    _process.Exited += Process_Exited;
                    ReadOutputAsync(_process);
                }
            }
            return _process;
        }
    }

    public bool EnableDebugLog
    {
        get;
        set;
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        //throw new NotImplementedException();
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        _logger.LogDebug($"{_process!.ProcessName}-{_process.Id}:{_process.ExitCode}");
    }

    public async Task<(bool Result, string Message)> RunAsync(string? path = null)
    {
        if (IsRunning())
        {
            return (false, $"{_metaDataProvider.ProcessName_App} has been running already!");
        }
        else
        {
            _process?.Dispose();
            if (string.IsNullOrEmpty(path))
            {
                path = await SearchPathAsync();
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException($"Can not find path of {_metaDataProvider.AppFileName}");
            }
            ProcessStartInfo startInfo = new()
            {
                FileName = Path.Combine(path, _metaDataProvider.AppFileName),
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            if(EnableDebugLog)
            {
                ///https://priyashpatil.com/posts/how-to-access-and-read-ollama-server-logs-on-various-systems#accessing-ollama-logs-on-windows
                startInfo.EnvironmentVariables.Add("OLLAMA_DEBUG", "1");
                _metaDataProvider.EnvironmentVariables = startInfo.EnvironmentVariables;
            }
            _process = new()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };
            _process.Exited += (sender, args) =>
            {
                _logger.LogDebug($"{_process.ProcessName}-{_process.Id}:{_process.ExitCode}");
            };
            _process.OutputDataReceived += (sender, args) =>
            {
                _logger.LogDebug($"{_process.ProcessName}-{_process.Id}:{args.Data}");
            };
            _process.ErrorDataReceived += (sender, args) =>
            {
                _logger.LogError($"{_process.ProcessName}-{_process.Id}:{args.Data}");
            };
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            return (true, $"{_metaDataProvider.AppFileName} is running:{_process.Id}");
        }
    }
    public async Task<(bool Result, string Message)> RebootAsync(TimeSpan waitExitTime)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(waitExitTime);
        await Terminate(cts.Token);
        return await RunAsync();
    }
    public async Task Terminate(CancellationToken? cancellationToken = null)
    {
        if (Process != null && !Process.HasExited)
        {
            Process.Kill();
            if (cancellationToken.HasValue)
            {
                await Process.WaitForExitAsync(cancellationToken.Value);
            }
        }
    }
    public bool IsRunning()
    {
        return Process != null;
    }
    public IObservable<string> TailLog()
    {
        return FileWatcher.TailLogFileRx(_metaDataProvider.ServerLogFilePath);
    }
    private Process? LinkProcess(string processName)
    {
        return Process.GetProcesses().FirstOrDefault(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
    }

    public Task<string?> SearchPathAsync()
    {
        return Task.Run(GetInstallPath);
    }
    public async Task<IEnumerable<string>> GetModels()
    {
        var models = await _ollamaApi.ListModels();
        return models.Models.Select(mi => mi.Name);
    }

    private static readonly string[] _possiblePaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ollama", "ollama.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ollama", "ollama.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ollama", "ollama.exe")
    ];

    public static bool IsOllamaInstalled()
    {
        return CheckExecutableExists() ||
               CheckEnvironmentVariable() ||
               CheckServiceRunning() ||
               CheckRegistry();
    }

    private static bool CheckExecutableExists()
    {
        foreach (var path in _possiblePaths)
        {
            if (File.Exists(path))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CheckEnvironmentVariable()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool CheckServiceRunning()
    {
        try
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Process.GetProcessesByName("ollama").Any();
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool CheckRegistry()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) return false;

        try
        {
            const string registryKey = @"SOFTWARE\Ollama";
            using var key = Registry.LocalMachine.OpenSubKey(registryKey);
            return key != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string? GetInstallPath()
    {
        foreach (var path in _possiblePaths)
        {
            if (File.Exists(path))
            {
                return Path.GetDirectoryName(path);
            }
        }

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where" + (Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : ""),
                    Arguments = "ollama",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(1000);
            if (process.ExitCode == 0)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                return Path.GetDirectoryName(output.Split('\n').First().Trim());
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return string.Empty;
    }
    static async Task ReadOutputAsync(Process process)
    {
        using System.IO.StreamReader reader = process.StandardOutput;
        while (!reader.EndOfStream)
        {
            string line = await reader.ReadLineAsync();
            if (line != null)
            {
                Console.WriteLine($"{process.ProcessName} Output: " + line);
            }
        }
    }
}