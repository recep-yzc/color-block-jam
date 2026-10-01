using ColorBlockJam.UI.Views;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlockPopupPresenter : ViewPresenter<BoosterUnlockPopup>
    {
        private readonly BoosterUnlocks unlocks;

        public BoosterUnlockPopupPresenter(BoosterUnlockPopup popup, BoosterUnlocks unlocks)
            : base(popup)
        {
            this.unlocks = unlocks;
        }

        protected override void OnInitialize()
        {
            View.ClaimButton.Clicked += unlocks.Claim;
        }

        protected override void OnDispose()
        {
            View.ClaimButton.Clicked -= unlocks.Claim;
        }

        protected override void OnShowing()
        {
            View.SetBooster(unlocks.Current);
        }
    }
}
