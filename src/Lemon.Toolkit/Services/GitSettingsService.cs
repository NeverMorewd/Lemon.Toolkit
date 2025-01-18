namespace Lemon.Toolkit.Services
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;

    public class GitSettingsService
    {
        private readonly ILogger _logger;
        public GitSettingsService(ILogger<GitSettingsService> logger)
        {
            _logger = logger;
        }
        public void EnableProxy(string proxyUrl)
        {
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                throw new ArgumentException("代理URL不能为空。");
            }

            SetGitConfig("http.proxy", proxyUrl);
            SetGitConfig("https.proxy", proxyUrl);
            _logger.LogInformation($"Git代理已启用: {proxyUrl}");
        }
        public void DisableProxy()
        {
            UnsetGitConfig("http.proxy");
            UnsetGitConfig("https.proxy");
            _logger.LogInformation("Git代理已禁用。");
        }
        public string GetCurrentProxy()
        {
            string httpProxy = GetGitConfig("http.proxy");
            string httpsProxy = GetGitConfig("https.proxy");

            if (!string.IsNullOrEmpty(httpProxy) || !string.IsNullOrEmpty(httpsProxy))
            {
                return $"当前Git代理设置:\nhttp.proxy: {httpProxy ?? "未设置"}\nhttps.proxy: {httpsProxy ?? "未设置"}";
            }
            else
            {
                return "当前未设置Git代理。";
            }
        }
        private string? GetGitConfig(string key)
        {
            return RunGitCommand($"config --global --get {key}", silent: true);
        }
        private void SetGitConfig(string key, string value)
        {
            RunGitCommand($"config --global {key} {value}");
        }
        private void UnsetGitConfig(string key)
        {
            RunGitCommand($"config --global --unset {key}");
        }
        private string? RunGitCommand(string arguments, bool silent = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();

                if (process.ExitCode != 0)
                {
                    if (!silent)
                    {
                        throw new InvalidOperationException($"Git命令执行失败: {error}");
                    }
                    return null;
                }

                return output;
            }
        }
    }
}
