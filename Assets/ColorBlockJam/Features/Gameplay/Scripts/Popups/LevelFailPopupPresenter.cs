using Framework.UI.Views;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelFailPopupPresenter : ViewPresenter<LevelFailPopup>
    {
        private readonly ILevelFlow flow;
        private readonly LevelOutcome outcome;

        public LevelFailPopupPresenter(LevelFailPopup popup, ILevelFlow flow, LevelOutcome outcome)
            : base(popup)
        {
            this.flow = flow;
            this.outcome = outcome;
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

        protected override void OnShowing()
        {
            View.SetReason(outcome.FailReason);
        }
    }
}
