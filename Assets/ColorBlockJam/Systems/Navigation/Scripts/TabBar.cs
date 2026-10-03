using System;
using System.Collections.Generic;
using ColorBlockJam.UI;
using LitMotion;
using UnityEngine;

namespace ColorBlockJam.Navigation
{
    public sealed class TabBar : MonoBehaviour
    {
        private const DrivenTransformProperties DrivenProperties =
            DrivenTransformProperties.Anchors | DrivenTransformProperties.Pivot | DrivenTransformProperties.AnchoredPosition |
            DrivenTransformProperties.SizeDelta;

        [Tooltip("Sekmelerin altında durduğu obje. Sekme çubuğu sekmeleri bunun içine yan yana dizer.")]
        [SerializeField] private RectTransform tabContainer;
        [Tooltip("Seçili sekmenin arkasına kayan vurgu. Sekme konteynerinin çocuğudur.")]
        [SerializeField] private RectTransform selectionHighlight;
        [Tooltip("Sekmelerle konteynerin kenarları arasındaki boşluk.")]
        [SerializeField] private RectOffset padding = new();
        [Tooltip("Yan yana iki sekme arasındaki boşluk.")]
        [SerializeField, Min(0f)] private float spacing;

        private readonly List<NavigationTab> tabs = new();
        private NavigationConfig config;
        private NavigationTab selectedTab;
        private int highlightFrom;
        private int highlightTo;
        private float highlightBlend;
        private bool isLayoutDirty;
        private float[] extraWidths;
        private float[] extraWidthsBeforeResize;
        private float[] centers;
        private float[] widths;
        private MotionHandle resizeMotion;
        private DrivenRectTransformTracker drivenTabs;

        public event Action<string> TabClicked;

        public void Initialize(NavigationConfig navigationConfig)
        {
            config = navigationConfig;
            NavigationOrder.CollectSorted(tabContainer, config, tabs);
            extraWidths = new float[tabs.Count];
            extraWidthsBeforeResize = new float[tabs.Count];
            centers = new float[tabs.Count];
            widths = new float[tabs.Count];

            drivenTabs.Clear();
            for (var i = 0; i < tabs.Count; i++)
            {
                tabs[i].transform.SetSiblingIndex(i);
                tabs[i].Clicked += OnTabClicked;
                tabs[i].SetSelected(false, config, instant: true);
                drivenTabs.Add(this, tabs[i].RectTransform, DrivenProperties);
            }

            selectionHighlight.SetAsFirstSibling();
            LayOut();
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
                if (instant)
                {
                    highlightFrom = index;
                    highlightTo = index;
                    highlightBlend = 0f;
                }
            }

            UpdateSeparators();
            ResizeTabs(instant);
        }

        public void MoveHighlight(int fromIndex, int toIndex, float blend)
        {
            highlightFrom = fromIndex;
            highlightTo = toIndex;
            highlightBlend = blend;
            ApplyHighlight();
        }

        private void Reset()
        {
            padding = new RectOffset(10, 10, 10, 0);
        }

        private void OnRectTransformDimensionsChange()
        {
            isLayoutDirty = tabs.Count > 0;
        }

        private void LateUpdate()
        {
            if (!isLayoutDirty)
            {
                return;
            }

            isLayoutDirty = false;
            LayOut();
        }

        private void OnDestroy()
        {
            resizeMotion.TryCancel();
            drivenTabs.Clear();
        }

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
            Array.Copy(extraWidths, extraWidthsBeforeResize, extraWidths.Length);

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
                extraWidths[i] = Mathf.Max(0f, Mathf.LerpUnclamped(extraWidthsBeforeResize[i], target, blend));
            }

            LayOut();
        }

        private void LayOut()
        {
            if (tabs.Count == 0)
            {
                return;
            }

            var rect = tabContainer.rect;
            var extra = 0f;
            for (var i = 0; i < tabs.Count; i++)
            {
                extra += extraWidths[i];
            }

            var share = (rect.width - padding.horizontal - spacing * (tabs.Count - 1) - extra) / tabs.Count;
            var left = rect.xMin + padding.left;
            var anchor = new Vector2(rect.center.x, (padding.bottom - padding.top) * 0.5f);
            for (var i = 0; i < tabs.Count; i++)
            {
                var width = Mathf.Max(0f, share + extraWidths[i]);
                centers[i] = left + width * 0.5f;
                widths[i] = width;

                var tab = tabs[i].RectTransform;
                tab.anchorMin = new Vector2(0.5f, 0f);
                tab.anchorMax = new Vector2(0.5f, 1f);
                tab.pivot = new Vector2(0.5f, 0.5f);
                tab.anchoredPosition = new Vector2(centers[i] - anchor.x, anchor.y);
                tab.sizeDelta = new Vector2(width, -padding.vertical);
                left += width + spacing;
            }

            ApplyHighlight();
        }

        private void ApplyHighlight()
        {
            var position = selectionHighlight.localPosition;
            position.x = Mathf.Lerp(centers[highlightFrom], centers[highlightTo], highlightBlend);
            selectionHighlight.localPosition = position;
            selectionHighlight.sizeDelta = new Vector2(Mathf.Lerp(widths[highlightFrom], widths[highlightTo], highlightBlend),
                selectionHighlight.sizeDelta.y);
        }

        private void OnTabClicked(NavigationTab tab)
        {
            TabClicked?.Invoke(tab.PageId);
        }
    }
}
