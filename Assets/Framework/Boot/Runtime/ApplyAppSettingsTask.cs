using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Core.Startup;
using UnityEngine;

namespace Framework.Boot
{
    public sealed class ApplyAppSettingsTask : IStartupTask
    {
        private readonly AppSettings settings;

        public ApplyAppSettingsTask(AppSettings settings)
        {
            this.settings = settings;
        }

        public UniTask RunAsync(CancellationToken cancellationToken)
        {
            Application.targetFrameRate = settings.TargetFrameRate;
            Input.multiTouchEnabled = settings.MultiTouchEnabled;
            Screen.sleepTimeout = settings.PreventScreenSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            return UniTask.CompletedTask;
        }
    }
}
