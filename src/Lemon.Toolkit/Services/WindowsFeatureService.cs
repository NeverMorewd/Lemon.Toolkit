using Lemon.Toolkit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Lemon.Toolkit.Services
{
    [SupportedOSPlatform("windows")]
    public class WindowsFeatureService
    {
        private readonly WindowsIdentity _windowsIdentity;
        private readonly WindowsPrincipal _principal;
        private readonly ILogger _logger;
        public WindowsFeatureService(ILogger<WindowsFeatureService> logger)
        {
            _logger = logger;
            _windowsIdentity = WindowsIdentity.GetCurrent();
            _principal = new(_windowsIdentity);
        }

        public bool IsRunAsAdmin
        {
            get
            {
                return _principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        public bool IsUpPermissionWithOutTip
        {
            get
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", false);
                if (key != null)
                {
                    var value = key.GetValue("ConsentPromptBehaviorAdmin");
                    if (value != null && value.ToString() == "0")
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool IsUACEnabled
        {
            get
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", false);
                if (key != null)
                {
                    var value = key.GetValue("EnableLUA");
                    if (value != null && value.ToString() == "1")
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public void RestartSelfAsAdmin()
        {
            ProcessStartInfo startInfo = new()
            {
                Verb = "runas",
                UseShellExecute = true,
                FileName = Environment.ProcessPath,
                Arguments = Environment.CommandLine,
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        public void RunAsAdmin(ProcessStartInfo startInfo)
        {
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
        }

        public bool IsInAdminGroup()
        {
            var claims = _principal.Claims;
            return claims.Any(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid" && c.Value == "S-1-5-32-544");
        }

        public void CreateTask(string aTaskName,
            string anAppPath,
            string anAuthor,
            int aDelay,
            string? aParam = null)
        {
            string fullTaskName = aTaskName;
            string appPath = anAppPath;
            using TaskService taskService = new();
            TaskDefinition taskDefinition = taskService.NewTask();
            taskDefinition.RegistrationInfo.Author = anAuthor;
            taskDefinition.RegistrationInfo.Description = "WindowsFeatureService Test";
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            LogonTrigger logonTrigger = new()
            {
                UserId = WindowsIdentity.GetCurrent().User!.Value,
                Delay = TimeSpan.FromSeconds(aDelay)
            };
            taskDefinition.Triggers.Add(logonTrigger);
            taskDefinition.Actions.Add(new ExecAction(appPath, aParam, null));
            taskService.RootFolder.RegisterTaskDefinition(fullTaskName, taskDefinition);
        }
        public void DeleteTask(string aTaskName)
        {
            try
            {
                using TaskService taskService = new();
                taskService.RootFolder.DeleteTask(aTaskName, false);
            }
            catch
            {
                //ignore
            }
        }

        public IEnumerable<string> EnumerateTask(int aMax)
        {
            using TaskService taskService = new();
            try
            {
                var tasks = taskService.RootFolder.Tasks;
                if (tasks.Any())
                {
                    if (tasks.Count() > aMax)
                    {
                        return tasks.Take(aMax).Select(task => $"{task.Name};{task.NextRunTime}");
                    }
                    else
                    {
                        return tasks.Select(task => $"{task.Name};{task.NextRunTime}");
                    }
                }
                return Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
        public bool CanRegisterTask()
        {
            try
            {
                var testName = "CanRegisterTask";
                CreateTask(testName, Environment.ProcessPath!, "Lemon", 1);
                _logger.LogInformation($"Create {testName}");
                DeleteTask(testName);
                _logger.LogInformation($"Delete {testName}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogDebug($"CanRegisterTask Error: {ex}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
