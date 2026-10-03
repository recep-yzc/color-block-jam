using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.UI.Windows
{
    public abstract class ShowcasePopupPresenter : WindowPresenter<ShowcasePopup, bool>
    {
        private Sprite icon;
        private string displayName;
        private string description;

        protected UniTask<bool> ShowAsync(Sprite iconSprite, string shownName, string shownDescription,
            CancellationToken cancellationToken)
        {
            icon = iconSprite;
            displayName = shownName;
            description = shownDescription;
            return OpenAsync(false, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            View.ConfirmButton.Clicked += OnConfirmClicked;
        }

        protected override void OnViewDestroyed()
        {
            View.ConfirmButton.Clicked -= OnConfirmClicked;
        }

        protected override void OnShowing()
        {
            View.Show(icon, displayName, description);
        }

        private void OnConfirmClicked()
        {
            Finish(true);
        }
    }
}
