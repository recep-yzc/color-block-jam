using System;
using System.Collections.Generic;
using LitMotion;
using UnityEngine;

namespace ColorBlockJam.Shared.Navigation
{
    [CreateAssetMenu(menuName = "Color Block Jam/Navigation/Navigation Config", fileName = "NavigationConfig")]
    public sealed class NavigationConfig : ScriptableObject
    {
        [Header("Pages")]
        [Tooltip("Page ids from left to right. Pages and tabs in the scene are sorted by this list.")]
        [SerializeField] private string[] pageOrder = { "home" };

        [Tooltip("The page that opens when the scene starts.")]
        [SerializeField, NavigationId] private string defaultPageId = "home";

        [Header("Swipe")]
        [Tooltip("Drag distance, as a part of the page width, that changes the page on release.")]
        [SerializeField, Range(0.05f, 0.5f)] private float swipeDistanceThreshold = 0.2f;

        [Tooltip("Drag speed, in canvas units per second, that changes the page on release even for a short drag.")]
        [SerializeField, Min(0f)] private float swipeSpeedThreshold = 1500f;

        [Tooltip("How much the content follows the pointer past the first and the last page. 0 = locked, 1 = free.")]
        [SerializeField, Range(0f, 1f)] private float edgeResistance = 0.3f;

        [Header("Page Transition")]
        [SerializeField, Min(0.01f)] private float pageTransitionDuration = 0.3f;
        [SerializeField] private Ease pageTransitionEase = Ease.OutCubic;

        [Header("Tabs")]
        [SerializeField, Min(1f)] private float selectedTabIconScale = 1.3f;
        [Tooltip("How far the icon of the selected tab moves up, in canvas units.")]
        [SerializeField] private float selectedTabIconLift = 40f;
        [SerializeField, Min(0.01f)] private float tabTransitionDuration = 0.2f;
        [SerializeField] private Ease tabTransitionEase = Ease.OutBack;

        public IReadOnlyList<string> PageOrder => pageOrder;
        public string DefaultPageId => defaultPageId;

        public float SwipeDistanceThreshold => swipeDistanceThreshold;
        public float SwipeSpeedThreshold => swipeSpeedThreshold;
        public float EdgeResistance => edgeResistance;

        public float PageTransitionDuration => pageTransitionDuration;
        public Ease PageTransitionEase => pageTransitionEase;

        public float SelectedTabIconScale => selectedTabIconScale;
        public float SelectedTabIconLift => selectedTabIconLift;
        public float TabTransitionDuration => tabTransitionDuration;
        public Ease TabTransitionEase => tabTransitionEase;

        /// <summary>
        /// Position of the page in <see cref="PageOrder"/>, or -1 when the id is unknown.
        /// </summary>
        public int GetOrder(string pageId)
        {
            return Array.IndexOf(pageOrder, pageId);
        }
    }
}
