using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
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
    }
}
