using Framework.UI.Views;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelCompletePopupPresenter : ViewPresenter<LevelCompletePopup>
    {
        private readonly ILevelFlow flow;
        private readonly LevelOutcome outcome;

        public LevelCompletePopupPresenter(LevelCompletePopup popup, ILevelFlow flow, LevelOutcome outcome)
            : base(popup)
        {
            this.flow = flow;
            this.outcome = outcome;
        }

        protected override void OnInitialize()
        {
            View.NextButton.Clicked += flow.PlayNext;
            View.HomeButton.Clicked += flow.GoHome;
        }

        protected override void OnDispose()
        {
            View.NextButton.Clicked -= flow.PlayNext;
            View.HomeButton.Clicked -= flow.GoHome;
        }

        protected override void OnShowing()
        {
            View.SetReward(outcome.Reward);
        }
    }
}
