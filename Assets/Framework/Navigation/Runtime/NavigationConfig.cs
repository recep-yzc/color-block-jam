using System;
using System.Collections.Generic;
using LitMotion;
using UnityEngine;

namespace Framework.Navigation
{
    [CreateAssetMenu(menuName = "Framework/Navigation/Navigation Config", fileName = "NavigationConfig")]
    public sealed class NavigationConfig : ScriptableObject
    {
        [Header("Pages")]
        [Tooltip("Soldan sağa sayfa kimlikleri. Sahnedeki sayfalar ve sekmeler bu sıraya göre dizilir.")]
        [SerializeField] private string[] pageOrder = { "home" };

        [Tooltip("Sahne açılınca gösterilen sayfa.")]
        [SerializeField, NavigationId] private string defaultPageId = "home";

        [Header("Swipe")]
        [Tooltip("Bırakınca sayfayı değiştiren sürükleme mesafesi, sayfa genişliğine oranla.")]
        [SerializeField, Range(0.05f, 0.5f)] private float swipeDistanceThreshold = 0.2f;

        [Tooltip("Kısa bir sürüklemede bile bırakınca sayfayı değiştiren hız, saniyede canvas birimi.")]
        [SerializeField, Min(0f)] private float swipeSpeedThreshold = 1500f;

        [Tooltip("İlk ve son sayfanın ötesinde içeriğin parmağı ne kadar izlediği. 0 = kilitli, 1 = serbest.")]
        [SerializeField, Range(0f, 1f)] private float edgeResistance = 0.3f;

        [Header("Page Transition")]
        [Tooltip("Sayfa geçişinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pageTransitionDuration = 0.3f;
        [Tooltip("Sayfa geçişinin eğrisi.")]
        [SerializeField] private Ease pageTransitionEase = Ease.OutCubic;

        [Header("Selected Tab Icon")]
        [Tooltip("Seçili sekmenin ikon ölçeği.")]
        [SerializeField, Min(0.1f)] private float selectedTabIconScale = 1.3f;
        [Tooltip("Seçili sekmenin ikonunun yukarı kalktığı mesafe, canvas birimi.")]
        [SerializeField] private float selectedTabIconLift = 40f;

        [Header("Selected Tab Label")]
        [Tooltip("Seçili sekmenin yazısının ölçeği.")]
        [SerializeField, Min(0.1f)] private float selectedTabLabelScale = 1f;
        [Tooltip("Seçili sekmenin yazısının yukarı kalktığı mesafe, canvas birimi.")]
        [SerializeField] private float selectedTabLabelLift;

        [Header("Selected Tab Width")]
        [Tooltip("Seçili sekmenin fazladan genişliği, canvas birimi. Diğer sekmeler çubuğun kalanını paylaşır.")]
        [SerializeField, Min(0f)] private float selectedTabExtraWidth;

        [Header("Tab Transition")]
        [Tooltip("Sekme geçişinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float tabTransitionDuration = 0.2f;
        [Tooltip("Sekme geçişinin eğrisi.")]
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
        public float SelectedTabLabelScale => selectedTabLabelScale;
        public float SelectedTabLabelLift => selectedTabLabelLift;
        public float SelectedTabExtraWidth => selectedTabExtraWidth;
        public float TabTransitionDuration => tabTransitionDuration;
        public Ease TabTransitionEase => tabTransitionEase;

        public int GetOrder(string pageId)
        {
            return Array.IndexOf(pageOrder, pageId);
        }
    }
}
