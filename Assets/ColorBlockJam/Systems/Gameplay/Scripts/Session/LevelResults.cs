using System;
using System.Threading;
using ColorBlockJam.Economy;
using ColorBlockJam.Level;
using ColorBlockJam.Progression;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelResults : IDisposable
    {
        private readonly ILevelProvider levels;
        private readonly IProgressionService progression;
        private readonly ICoinWallet wallet;
        private readonly EconomyConfig economy;
        private readonly IWindows windows;
        private readonly ILevelFlow flow;
        private readonly GameplayConfig config;
        private readonly CancellationTokenSource lifetime = new();

        public LevelResults(ILevelProvider levels, IProgressionService progression, ICoinWallet wallet, EconomyConfig economy,
            IWindows windows, ILevelFlow flow, GameplayConfig config)
        {
            this.levels = levels;
            this.progression = progression;
            this.wallet = wallet;
            this.economy = economy;
            this.windows = windows;
            this.flow = flow;
            this.config = config;
        }

        public void Win(LevelDifficulty difficulty)
        {
            if (!levels.IsEditorTest)
            {
                progression.CompleteCurrentLevel();
            }

            var reward = economy.RewardFor(difficulty);
            wallet.Add(reward);
            ShowWinAsync(reward).Forget();
        }

        public void Fail(LevelFailReason reason)
        {
            ShowFailAsync(reason).Forget();
        }

        public async UniTask<int> OfferExtraTimeAsync(CancellationToken cancellationToken)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.Token);
            await WaitBeforeResultAsync(linked.Token);
            var offer = new ExtraTimeOffer(config.ExtraTimeSeconds, config.ExtraTimeCost);
            var isBought = await windows.Get<OutOfTimePopupPresenter>().ShowAsync(offer, linked.Token);
            return isBought ? offer.Seconds : 0;
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid ShowWinAsync(int reward)
        {
            var token = lifetime.Token;
            var (isCanceled, isNext) = await ShowWinWindowAsync(reward, token).SuppressCancellationThrow();
            if (!isCanceled && isNext)
            {
                flow.PlayNext();
            }
        }

        private async UniTask<bool> ShowWinWindowAsync(int reward, CancellationToken token)
        {
            await WaitBeforeResultAsync(token);
            return await windows.Get<LevelCompletePopupPresenter>().ShowAsync(reward, token);
        }

        private async UniTaskVoid ShowFailAsync(LevelFailReason reason)
        {
            var token = lifetime.Token;
            var (isCanceled, choice) = await ShowFailWindowAsync(reason, token).SuppressCancellationThrow();
            if (isCanceled)
            {
                return;
            }

            if (choice == LevelFailChoice.Home)
            {
                flow.GoHome();
            }
            else
            {
                flow.Restart();
            }
        }

        private async UniTask<LevelFailChoice> ShowFailWindowAsync(LevelFailReason reason, CancellationToken token)
        {
            await WaitBeforeResultAsync(token);
            return await windows.Get<LevelFailPopupPresenter>().ShowAsync(reason, token);
        }

        private UniTask WaitBeforeResultAsync(CancellationToken token)
        {
            return UniTask.Delay(TimeSpan.FromSeconds(config.ResultPopupDelay), cancellationToken: token);
        }
    }
}
