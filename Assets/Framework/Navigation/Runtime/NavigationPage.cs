using UnityEngine;

namespace Framework.Navigation
{
    /// <summary>
    /// A full screen page shown by <see cref="PageNavigator"/>.
    /// Drop the prefab under the navigator content and pick its id; the navigator places it.
    /// </summary>
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
