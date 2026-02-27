using TaskScheduler;

namespace PopupBlocker.Core.Services
{
    public class AutoRunService : IService
    {
        #region IService
        public bool Switch
        {
            get => GetTask(out _, out _);
            set
            {
                if (value)
                    RegisterTask();
                else
                    UnregisterTask(out _, out _);
            }
        }
        #endregion

        private static void RegisterTask()
        {
            UnregisterTask(out var taskScheduler, out var rootFolder);

            // 创建任务定义
            var taskDefinition = taskScheduler.NewTask(0);

            // 设置任务基本信息
            taskDefinition.RegistrationInfo.Description = "轻量级弹窗拦截器的开机自启任务";
            taskDefinition.RegistrationInfo.Author = "yiyiofficial.top";

            // 设置权限
            taskDefinition.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
            taskDefinition.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;

            // 设置触发器
            var triggers = taskDefinition.Triggers;
            var logonTrigger = (ILogonTrigger)triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
            logonTrigger.UserId = $"{Environment.UserDomainName}\\{Environment.UserName}";
            logonTrigger.Enabled = true;
            logonTrigger.Delay = "PT3S";

            // 设置操作
            var actions = taskDefinition.Actions;
            var execAction = (IExecAction)actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
            execAction.Path = AppPath.ExecutingPath;
            execAction.Arguments = AppPath.AutoRunSwitchProperty;

            // 设置设置，emmm，气笑了
            taskDefinition.Settings.Priority = 4;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.RunOnlyIfNetworkAvailable = false;
            taskDefinition.Settings.WakeToRun = false;
            taskDefinition.Settings.Enabled = true;
            taskDefinition.Settings.Hidden = false;
            taskDefinition.Settings.AllowHardTerminate = true;
            taskDefinition.Settings.RunOnlyIfIdle = false;

            // 注册任务
            rootFolder.RegisterTaskDefinition(
                AppPath.AppName,
                taskDefinition,
                (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                null,
                null,
                _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                null);
        }

        private static void UnregisterTask(out TaskSchedulerClass taskScheduler, out ITaskFolder rootFolder)
        {
            if (GetTask(out taskScheduler, out rootFolder))
                rootFolder.DeleteTask(AppPath.AppName, 0);
        }

        private static bool GetTask(out TaskSchedulerClass taskScheduler, out ITaskFolder rootFolder)
        {
            taskScheduler = new TaskSchedulerClass();
            taskScheduler.Connect(null, null, null, null);
            rootFolder = taskScheduler.GetFolder("\\");
            try
            {
                rootFolder.GetTask(AppPath.AppName);
                return true;
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
        }
    }
}
