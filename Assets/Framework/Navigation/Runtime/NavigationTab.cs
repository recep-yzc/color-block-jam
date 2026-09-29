using System;
using Framework.UI;
using Framework.UI.Buttons;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Framework.Navigation
{
    /// <summary>
    /// A button in the <see cref="TabBar"/> that opens the page with the same id.
    /// Drop the prefab under the tab bar and pick its id; the tab bar places it.
    /// A locked tab keeps its place in the order but only gives touch feedback, so it needs no page.
    /// </summary>
    public sealed class NavigationTab : ButtonBase, INavigationItem
    {
        [SerializeField, NavigationId] private string pageId;
        [SerializeField] private RectTransform icon;
        [Tooltip("Optional. Shown only while the tab is selected.")]
        [SerializeField] private GameObject selectedLabel;
        [Tooltip("A locked tab does not open a page.")]
        [SerializeField] private bool isLocked;
        [Tooltip("Optional. Divider on the right edge. The tab bar hides it next to the selected tab and after the last tab.")]
        [SerializeField] private GameObject separator;

        private Vector2 iconRestPosition;
        private Vector2 labelRestPosition;
        private MotionHandle iconScaleMotion;
        private MotionHandle iconLiftMotion;
        private MotionHandle labelScaleMotion;
        private MotionHandle labelLiftMotion;

        public event Action<NavigationTab> Clicked;

        public string PageId => pageId;
        public RectTransform RectTransform => (RectTransform)transform;

        private RectTransform Label => selectedLabel != null ? (RectTransform)selectedLabel.transform : null;

        protected override void Awake()
        {
            base.Awake();
            iconRestPosition = icon.anchoredPosition;

            if (Label != null)
            {
                labelRestPosition = Label.anchoredPosition;
            }
        }

        private void OnDestroy()
        {
            iconScaleMotion.TryCancel();
            iconLiftMotion.TryCancel();
            labelScaleMotion.TryCancel();
            labelLiftMotion.TryCancel();
        }

        public void SetSelected(bool selected, NavigationConfig config, bool instant)
        {
            Animate(icon, iconRestPosition, ref iconScaleMotion, ref iconLiftMotion,
                selected ? config.SelectedTabIconScale : 1f,
                selected ? config.SelectedTabIconLift : 0f,
                config, instant);

            if (Label == null)
            {
                return;
            }

            // The label appears from its rest place and disappears at once.
            selectedLabel.SetActive(selected);
            Animate(Label, labelRestPosition, ref labelScaleMotion, ref labelLiftMotion,
                selected ? config.SelectedTabLabelScale : 1f,
                selected ? config.SelectedTabLabelLift : 0f,
                config, instant || !selected);
        }

        public void SetSeparatorVisible(bool visible)
        {
            if (separator != null)
            {
                separator.SetActive(visible);
            }
        }

        private static void Animate(RectTransform target, Vector2 restPosition, ref MotionHandle scaleMotion, ref MotionHandle liftMotion,
            float scale, float lift, NavigationConfig config, bool instant)
        {
            scaleMotion.TryCancel();
            liftMotion.TryCancel();

            var targetScale = Vector3.one * scale;
            var targetY = restPosition.y + lift;

            if (instant)
            {
                target.localScale = targetScale;
                target.anchoredPosition = new Vector2(restPosition.x, targetY);
                return;
            }

            scaleMotion = LMotion.Create(target.localScale, targetScale, config.TabTransitionDuration)
                .WithEase(config.TabTransitionEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target);

            liftMotion = LMotion.Create(target.anchoredPosition.y, targetY, config.TabTransitionDuration)
                .WithEase(config.TabTransitionEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAnchoredPositionY(target);
        }

        protected override void OnClick()
        {
            if (!isLocked)
            {
                Clicked?.Invoke(this);
            }
        }
    }
}
