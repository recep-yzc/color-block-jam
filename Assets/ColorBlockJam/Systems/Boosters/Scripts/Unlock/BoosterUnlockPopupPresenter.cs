using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlockPopupPresenter : ShowcasePopupPresenter
    {
        public UniTask<bool> ShowAsync(BoosterDefinition booster, CancellationToken cancellationToken = default)
        {
            return ShowAsync(booster.Icon, booster.DisplayName, booster.Description, cancellationToken);
        }
    }
}
