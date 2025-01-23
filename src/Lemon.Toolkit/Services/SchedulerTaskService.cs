using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Lemon.Toolkit.Services
{
    [SupportedOSPlatform("windows")]
    public class SchedulerTaskService
    {
        public const string CHILD_SESSION_CoreServer_TASK = "CoreServerStartTask";
        public const string CHILD_SESSION_Unlock_TASK = "CHILD_SESSION_Unlock_TASK";
        public void CreateTask(string aTaskName,
            string anAppPath,
            string anAuthor,
            int aDelay,
            string aParam = null)
        {
            string fullTaskName = aTaskName;
            string appPath = anAppPath;
            using TaskService taskService = new();
            TaskDefinition taskDefinition = taskService.NewTask();
            taskDefinition.RegistrationInfo.Author = anAuthor;
            taskDefinition.RegistrationInfo.Description = "For II.RPA.ZDeskTop";
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            LogonTrigger logonTrigger = new LogonTrigger()
            {
                UserId = WindowsIdentity.GetCurrent().User.Value,
                Delay = TimeSpan.FromSeconds(aDelay)
            };
            //var bt = new BootTrigger { Delay = new TimeSpan(0, 0, 1) };
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
                using TaskService taskService = new();
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.RegistrationInfo.Author = "Test";
                taskDefinition.RegistrationInfo.Description = "Temporary task to check permissions";

                taskDefinition.Triggers.Add(new TimeTrigger { StartBoundary = DateTime.Now.AddDays(999) });
                taskDefinition.Actions.Add(new ExecAction("cmd.exe"));

                var taskId = "TempTask_" + Guid.NewGuid();
                taskService.RootFolder.RegisterTaskDefinition(taskId, taskDefinition);
                taskService.RootFolder.DeleteTask(taskId);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
