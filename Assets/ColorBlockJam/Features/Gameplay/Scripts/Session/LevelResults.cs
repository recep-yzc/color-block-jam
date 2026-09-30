using System;
using System.Threading;
using ColorBlockJam.Economy;
using ColorBlockJam.Progression;
using Cysharp.Threading.Tasks;
using Framework.UI.Popups;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelResults : IDisposable
    {
        private readonly ILevelProvider levels;
        private readonly IProgressionService progression;
        private readonly ICoinWallet wallet;
        private readonly EconomyConfig economy;
        private readonly LevelOutcome outcome;
        private readonly IPopupService popups;
        private readonly GameplayConfig config;
        private readonly CancellationTokenSource lifetime = new();

        public LevelResults(ILevelProvider levels, IProgressionService progression, ICoinWallet wallet, EconomyConfig economy,
            LevelOutcome outcome, IPopupService popups, GameplayConfig config)
        {
            this.levels = levels;
            this.progression = progression;
            this.wallet = wallet;
            this.economy = economy;
            this.outcome = outcome;
            this.popups = popups;
            this.config = config;
        }

        public void Win()
        {
            if (!levels.IsEditorTest)
            {
                progression.CompleteCurrentLevel();
            }

            var reward = economy.LevelCompleteReward;
            wallet.Add(reward);
            outcome.Win(reward);
            ShowAsync<LevelCompletePopup>().Forget();
        }

        public void Fail(LevelFailReason reason)
        {
            outcome.Fail(reason);
            ShowAsync<LevelFailPopup>().Forget();
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid ShowAsync<TPopup>() where TPopup : Popup
        {
            var isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(config.ResultPopupDelay), cancellationToken: lifetime.Token)
                .SuppressCancellationThrow();
            if (!isCanceled)
            {
                popups.ShowAsync<TPopup>(lifetime.Token).Forget();
            }
        }
    }
}
