using System;
using System.Threading;
using ColorBlockJam.Economy;
using ColorBlockJam.Progression;
using Cysharp.Threading.Tasks;
using Framework.UI.Popups;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// What ending a level does: a win moves the player's progress on and pays the reward, and either end shows its
    /// result popup after a short delay, so the last moves can finish on screen first.
    /// </summary>
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
            // A level tried from the level editor is not part of the player's progress.
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
