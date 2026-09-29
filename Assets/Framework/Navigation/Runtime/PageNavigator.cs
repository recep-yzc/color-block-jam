using System;
using System.Collections.Generic;
using Framework.UI;
using LitMotion;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Navigation
{
    /// <summary>
    /// Places the <see cref="NavigationPage"/>s side by side, sorted by the config,
    /// and moves between them by code or by a horizontal drag (mouse or touch).
    /// Put it on the viewport; the viewport needs a raycast target and a RectMask2D.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class PageNavigator : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Parent of the pages. It moves left and right to show the current page.")]
        [SerializeField] private RectTransform content;

        private readonly List<NavigationPage> pages = new();
        private NavigationConfig config;
        private Canvas rootCanvas;
        private MotionHandle transition;
        private int currentIndex = -1;
        private float pageWidth;
        private float dragSpeed;

        /// <summary>Raised when a page starts to open, before the transition.</summary>
        public event Action<NavigationPage> PageOpening;

        /// <summary>Raised when the content moves. The value is in pages: 0 is the first page, 1 the second.</summary>
        public event Action<float> ScrollPositionChanged;

        public IReadOnlyList<NavigationPage> Pages => pages;

        public void Initialize(NavigationConfig navigationConfig)
        {
            config = navigationConfig;
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

            NavigationOrder.CollectSorted(content, config, pages);
            for (var i = 0; i < pages.Count; i++)
            {
                pages[i].transform.SetSiblingIndex(i);
            }

            LayoutPages();
        }

        public bool Open(string pageId, bool instant)
        {
            var index = pages.FindIndex(page => page.PageId == pageId);
            if (index < 0)
            {
                Debug.LogWarning($"There is no page with id '{pageId}' under {name}.", this);
                return false;
            }

            OpenAt(index, instant);
            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transition.TryCancel();
            dragSpeed = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var delta = eventData.delta.x / rootCanvas.scaleFactor;
            var position = content.anchoredPosition.x + delta;

            if (position > 0f || position < LastPagePosition)
            {
                position = content.anchoredPosition.x + delta * config.EdgeResistance;
            }

            SetContentPosition(position);

            if (Time.unscaledDeltaTime > 0f)
            {
                dragSpeed = delta / Time.unscaledDeltaTime;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Positive offset means the content moved toward the next page.
            var offset = -content.anchoredPosition.x / pageWidth - currentIndex;
            var targetIndex = currentIndex;

            if (offset > config.SwipeDistanceThreshold || dragSpeed < -config.SwipeSpeedThreshold)
            {
                targetIndex++;
            }
            else if (offset < -config.SwipeDistanceThreshold || dragSpeed > config.SwipeSpeedThreshold)
            {
                targetIndex--;
            }

            OpenAt(Mathf.Clamp(targetIndex, 0, pages.Count - 1), instant: false);
        }

        private float LastPagePosition => -(pages.Count - 1) * pageWidth;

        private void OnRectTransformDimensionsChange()
        {
            if (config != null)
            {
                LayoutPages();
            }
        }

        private void OnDestroy()
        {
            transition.TryCancel();
        }

        private void LayoutPages()
        {
            pageWidth = ((RectTransform)transform).rect.width;

            for (var i = 0; i < pages.Count; i++)
            {
                var page = pages[i].RectTransform;
                page.anchorMin = Vector2.zero;
                page.anchorMax = Vector2.up;
                page.pivot = new Vector2(0f, 0.5f);
                page.sizeDelta = new Vector2(pageWidth, 0f);
                page.anchoredPosition = new Vector2(i * pageWidth, 0f);
            }

            if (currentIndex >= 0)
            {
                MoveContentTo(currentIndex, instant: true);
            }
        }

        private void OpenAt(int index, bool instant)
        {
            if (index != currentIndex)
            {
                currentIndex = index;
                pages[index].NotifyOpening();
                PageOpening?.Invoke(pages[index]);
            }

            MoveContentTo(index, instant);
        }

        private void MoveContentTo(int index, bool instant)
        {
            transition.TryCancel();
            var target = -index * pageWidth;

            if (instant)
            {
                SetContentPosition(target);
                return;
            }

            transition = LMotion.Create(content.anchoredPosition.x, target, config.PageTransitionDuration)
                .WithEase(config.PageTransitionEase)
                .WithScheduler(UIMotion.Scheduler)
                .Bind(this, static (position, navigator) => navigator.SetContentPosition(position));
        }

        private void SetContentPosition(float position)
        {
            content.anchoredPosition = new Vector2(position, content.anchoredPosition.y);
            ScrollPositionChanged?.Invoke(pageWidth > 0f ? -position / pageWidth : 0f);
        }
    }
}
