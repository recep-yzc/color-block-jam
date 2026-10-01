using UnityEngine;

namespace ColorBlockJam.Navigation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class NavigationPage : MonoBehaviour, INavigationItem
    {
        [Tooltip("Bu sayfanın kimliği. Navigation ayarlarındaki sayfa sırasında olmalı.")]
        [SerializeField, NavigationId] private string pageId;

        private IPageOpeningListener[] openingListeners;

        public string PageId => pageId;
        public RectTransform RectTransform => (RectTransform)transform;

        internal void NotifyOpening()
        {
            openingListeners ??= GetComponentsInChildren<IPageOpeningListener>(true);

            foreach (var listener in openingListeners)
            {
                listener.OnPageOpening();
            }
        }
    }
}
