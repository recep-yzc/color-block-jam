using System;
using System.Collections.Generic;
using Framework.UI;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Navigation
{
    /// <summary>
    /// Holds the <see cref="NavigationTab"/>s, sorts them by the config, widens the selected tab
    /// and moves the selection highlight.
    /// </summary>
    public sealed class TabBar : MonoBehaviour
    {
        [Tooltip("Parent of the tabs. Its layout group places the tabs side by side.")]
        [SerializeField] private RectTransform tabContainer;
        [Tooltip("Child of the tab container that ignores layout and slides behind the selected tab.")]
        [SerializeField] private RectTransform selectionHighlight;

        private readonly List<NavigationTab> tabs = new();
        private NavigationConfig config;
        private NavigationTab selectedTab;
        private int highlightFrom;
        private int highlightTo;
        private float highlightBlend;
        private bool isLayoutDirty;
        private LayoutElement[] tabLayouts;
        private float[] widthsBeforeResize;
        private MotionHandle resizeMotion;

        public event Action<string> TabClicked;

        public void Initialize(NavigationConfig navigationConfig)
        {
            config = navigationConfig;
            NavigationOrder.CollectSorted(tabContainer, config, tabs);
            tabLayouts = new LayoutElement[tabs.Count];
            widthsBeforeResize = new float[tabs.Count];

            for (var i = 0; i < tabs.Count; i++)
            {
                tabs[i].transform.SetSiblingIndex(i);
                tabs[i].Clicked += OnTabClicked;
                tabs[i].SetSelected(false, config, instant: true);

                // The selected tab gets its extra width as preferred width; the layout group shares the rest.
                tabLayouts[i] = tabs[i].TryGetComponent(out LayoutElement layout) ? layout : tabs[i].gameObject.AddComponent<LayoutElement>();
                tabLayouts[i].preferredWidth = 0f;
            }

            selectionHighlight.SetAsFirstSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tabContainer);
        }

        public int IndexOf(string pageId)
        {
            return tabs.FindIndex(tab => tab.PageId == pageId);
        }

        public void Select(string pageId, bool instant)
        {
            var index = IndexOf(pageId);
            var tab = index >= 0 ? tabs[index] : null;

            if (selectedTab != null && selectedTab != tab)
            {
                selectedTab.SetSelected(false, config, instant);
            }

            selectedTab = tab;
            if (selectedTab != null)
            {
                selectedTab.SetSelected(true, config, instant);
            }

            UpdateSeparators();
            ResizeTabs(instant);
        }

        /// <summary>
        /// Places the highlight between two tabs. <paramref name="blend"/> 0 is on <paramref name="fromIndex"/>, 1 is on <paramref name="toIndex"/>.
        /// </summary>
        public void MoveHighlight(int fromIndex, int toIndex, float blend)
        {
            highlightFrom = fromIndex;
            highlightTo = toIndex;
            highlightBlend = blend;
            ApplyHighlight();
        }

        private void OnRectTransformDimensionsChange()
        {
            // The layout group moves the tabs later in this frame; place the highlight after it.
            isLayoutDirty = tabs.Count > 0;
        }

        private void LateUpdate()
        {
            if (!isLayoutDirty)
            {
                return;
            }

            isLayoutDirty = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(tabContainer);
            ApplyHighlight();
        }

        private void OnDestroy()
        {
            resizeMotion.TryCancel();
        }

        // A separator sits on the right edge of its tab, so the selected tab hides its own and its left neighbour's.
        private void UpdateSeparators()
        {
            for (var i = 0; i < tabs.Count; i++)
            {
                var isLast = i == tabs.Count - 1;
                var touchesSelection = tabs[i] == selectedTab || (!isLast && tabs[i + 1] == selectedTab);
                tabs[i].SetSeparatorVisible(!isLast && !touchesSelection);
            }
        }

        private void ResizeTabs(bool instant)
        {
            resizeMotion.TryCancel();

            for (var i = 0; i < tabs.Count; i++)
            {
                widthsBeforeResize[i] = tabLayouts[i].preferredWidth;
            }

            if (instant)
            {
                ApplyWidths(1f);
                return;
            }

            resizeMotion = LMotion.Create(0f, 1f, config.TabTransitionDuration)
                .WithEase(config.TabTransitionEase)
                .WithScheduler(UIMotion.Scheduler)
                .Bind(this, static (blend, bar) => bar.ApplyWidths(blend));
        }

        private void ApplyWidths(float blend)
        {
            for (var i = 0; i < tabs.Count; i++)
            {
                var target = tabs[i] == selectedTab ? config.SelectedTabExtraWidth : 0f;
                tabLayouts[i].preferredWidth = Mathf.Max(0f, Mathf.LerpUnclamped(widthsBeforeResize[i], target, blend));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(tabContainer);
            ApplyHighlight();
        }

        private void ApplyHighlight()
        {
            var from = tabs[highlightFrom].RectTransform;
            var to = tabs[highlightTo].RectTransform;

            var position = selectionHighlight.localPosition;
            position.x = Mathf.Lerp(from.localPosition.x, to.localPosition.x, highlightBlend);
            selectionHighlight.localPosition = position;
            selectionHighlight.sizeDelta = new Vector2(Mathf.Lerp(from.rect.width, to.rect.width, highlightBlend), selectionHighlight.sizeDelta.y);
        }

        private void OnTabClicked(NavigationTab tab)
        {
            TabClicked?.Invoke(tab.PageId);
        }
    }
}
