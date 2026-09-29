using Framework.UI.Views;

namespace ColorBlockJam.Gameplay
{
    public sealed class PausePopupPresenter : ViewPresenter<PausePopup>
    {
        private readonly ILevelFlow flow;

        public PausePopupPresenter(PausePopup popup, ILevelFlow flow)
            : base(popup)
        {
            this.flow = flow;
        }

        protected override void OnInitialize()
        {
            View.RestartButton.Clicked += flow.Restart;
            View.HomeButton.Clicked += flow.GoHome;
        }

        protected override void OnDispose()
        {
            View.RestartButton.Clicked -= flow.Restart;
            View.HomeButton.Clicked -= flow.GoHome;
        }
    }
}
