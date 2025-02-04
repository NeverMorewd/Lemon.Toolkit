using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Lemon.Toolkit.Services.OllamaServices;

public class OllamaManageService
{
    private readonly ILogger _logger;
    private readonly IOllamaApi _ollamaApi;
    public OllamaManageService(IOllamaApi ollamaApi, ILogger<OllamaManageService> logger)
    {
        _logger = logger;
        _ollamaApi = ollamaApi;
    }
    

    public Task<string?> GetPath()
    {
        return Task.Run(GetInstallPath); 
    }
    public async Task<IEnumerable<string>> GetModels()
    {
        var models = await _ollamaApi.ListModels();
        return models.Models.Select(mi=>mi.Name);
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
}