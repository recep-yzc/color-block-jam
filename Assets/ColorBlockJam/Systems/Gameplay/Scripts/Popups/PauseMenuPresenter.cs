using ColorBlockJam.Settings;

namespace ColorBlockJam.Gameplay
{
    public sealed class PauseMenuPresenter : SettingsPopupPresenter
    {
        public PauseMenuPresenter(ISettingsService settings, IHapticService haptics)
            : base(settings, haptics)
        {
        }
    }
}
