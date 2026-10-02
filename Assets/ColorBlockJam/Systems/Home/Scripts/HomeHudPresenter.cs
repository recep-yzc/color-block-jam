using ColorBlockJam.Settings;
using ColorBlockJam.UI.Views;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Home
{
    public sealed class HomeHudPresenter : ViewPresenter<HomeHudView>
    {
        private readonly IWindows windows;

        public HomeHudPresenter(HomeHudView view, IWindows windows)
            : base(view)
        {
            this.windows = windows;
        }

        protected override void OnInitialize()
        {
            View.SettingsButton.Clicked += OnSettingsClicked;
        }

        protected override void OnDispose()
        {
            View.SettingsButton.Clicked -= OnSettingsClicked;
        }

        private void OnSettingsClicked()
        {
            windows.Get<SettingsPopupPresenter>().ShowAsync().Forget();
        }
    }
}
