using ColorBlockJam.Economy;
using Framework.UI.Views;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class BoosterBarPresenter : ViewPresenter<BoosterBarView>, ITickable
    {
        private readonly LevelSession session;
        private readonly FreezeBooster freeze;
        private readonly HammerBooster hammer;
        private readonly ICoinWallet wallet;
        private bool wasFrozen;

        public BoosterBarPresenter(BoosterBarView view, LevelSession session, FreezeBooster freeze, HammerBooster hammer,
            ICoinWallet wallet)
            : base(view)
        {
            this.session = session;
            this.freeze = freeze;
            this.hammer = hammer;
            this.wallet = wallet;
        }

        protected override void OnInitialize()
        {
            View.SetPrices(freeze.Cost, hammer.Cost);
            View.SetHammerAiming(hammer.IsAiming);
            View.FreezeButton.Clicked += freeze.Use;
            View.HammerButton.Clicked += hammer.ToggleAim;
            hammer.AimingChanged += OnAimingChanged;
            session.StateChanged += OnStateChanged;
            wallet.CoinsChanged += OnCoinsChanged;
            Refresh();
        }

        protected override void OnDispose()
        {
            View.FreezeButton.Clicked -= freeze.Use;
            View.HammerButton.Clicked -= hammer.ToggleAim;
            hammer.AimingChanged -= OnAimingChanged;
            session.StateChanged -= OnStateChanged;
            wallet.CoinsChanged -= OnCoinsChanged;
        }

        public void Tick()
        {
            if (session.Timer.IsFrozen != wasFrozen)
            {
                Refresh();
            }
        }

        private void OnStateChanged()
        {
            if (session.State != LevelState.Playing)
            {
                hammer.PutBack();
            }

            Refresh();
        }

        private void OnCoinsChanged(int coins)
        {
            if (hammer.IsAiming && !hammer.CanAfford)
            {
                hammer.PutBack();
            }

            Refresh();
        }

        private void OnAimingChanged()
        {
            View.SetHammerAiming(hammer.IsAiming);
            Refresh();
        }

        private void Refresh()
        {
            wasFrozen = session.Timer.IsFrozen;
            View.FreezeButton.Interactable = freeze.CanUse;

            View.HammerButton.Interactable = session.State == LevelState.Playing && (hammer.IsAiming || hammer.CanAfford);
        }
    }
}
