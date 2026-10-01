using System.Threading;
using ColorBlockJam.Core.Startup;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Boot
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
            Screen.sleepTimeout = settings.PreventScreenSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            return UniTask.CompletedTask;
        }
    }
}
