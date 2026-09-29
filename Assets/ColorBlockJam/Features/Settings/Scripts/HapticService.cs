using UnityEngine;

namespace ColorBlockJam.Settings
{
    public sealed class HapticService : IHapticService
    {
        private readonly ISettingsService settings;

        public HapticService(ISettingsService settings)
        {
            this.settings = settings;
        }

        public void Play()
        {
            if (!settings.IsEnabled(SettingKind.Haptic))
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
