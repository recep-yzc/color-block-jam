using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Shared.Navigation
{
    internal static class NavigationOrder
    {
        /// <summary>
        /// Collects the items that are direct children of <paramref name="parent"/> and sorts them by
        /// <see cref="NavigationConfig.PageOrder"/>. Items with an unknown id keep their hierarchy order and go to the end.
        /// </summary>
        public static void CollectSorted<T>(Transform parent, NavigationConfig config, List<T> result)
            where T : Component, INavigationItem
        {
            result.Clear();

            for (var i = 0; i < parent.childCount; i++)
            {
                if (!parent.GetChild(i).TryGetComponent(out T item))
                {
                    continue;
                }

                if (config.GetOrder(item.PageId) < 0)
                {
                    Debug.LogWarning($"'{item.name}' uses page id '{item.PageId}', which is not in {config.name}.", item);
                }

                result.Add(item);
            }

            result.Sort((a, b) =>
            {
                var byConfig = SortKey(config, a).CompareTo(SortKey(config, b));
                return byConfig != 0 ? byConfig : a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
            });
        }

        private static int SortKey(NavigationConfig config, INavigationItem item)
        {
            var order = config.GetOrder(item.PageId);
            return order < 0 ? int.MaxValue : order;
        }
    }
}
