using System;
using ColorBlockJam.Shared.UI;
using ColorBlockJam.Shared.UI.Buttons;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Shared.Navigation
{
    /// <summary>
    /// A button in the <see cref="TabBar"/> that opens the page with the same id.
    /// Drop the prefab under the tab bar and pick its id; the tab bar places it.
    /// </summary>
    public sealed class NavigationTab : ButtonBase, INavigationItem
    {
        [SerializeField, NavigationId] private string pageId;
        [SerializeField] private RectTransform icon;
        [Tooltip("Optional. Shown only while the tab is selected.")]
        [SerializeField] private GameObject selectedLabel;

        private Vector2 iconRestPosition;
        private MotionHandle iconScaleMotion;
        private MotionHandle iconLiftMotion;

        public event Action<NavigationTab> Clicked;

        public string PageId => pageId;
        public RectTransform RectTransform => (RectTransform)transform;

        protected override void Awake()
        {
            base.Awake();
            iconRestPosition = icon.anchoredPosition;
        }

        private void OnDestroy()
        {
            iconScaleMotion.TryCancel();
            iconLiftMotion.TryCancel();
        }

        public void SetSelected(bool selected, NavigationConfig config, bool instant)
        {
            var targetScale = selected ? config.SelectedTabIconScale : 1f;
            var targetY = iconRestPosition.y + (selected ? config.SelectedTabIconLift : 0f);

            if (selectedLabel != null)
            {
                selectedLabel.SetActive(selected);
            }

            iconScaleMotion.TryCancel();
            iconLiftMotion.TryCancel();

            if (instant)
            {
                icon.localScale = Vector3.one * targetScale;
                icon.anchoredPosition = new Vector2(iconRestPosition.x, targetY);
                return;
            }

            iconScaleMotion = LMotion.Create(icon.localScale, Vector3.one * targetScale, config.TabTransitionDuration)
                .WithEase(config.TabTransitionEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(icon);

            iconLiftMotion = LMotion.Create(icon.anchoredPosition.y, targetY, config.TabTransitionDuration)
                .WithEase(config.TabTransitionEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAnchoredPositionY(icon);
        }

        protected override void OnClick()
        {
            Clicked?.Invoke(this);
        }
    }
}
