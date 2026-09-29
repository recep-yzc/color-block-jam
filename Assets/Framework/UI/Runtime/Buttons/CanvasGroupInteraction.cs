using UnityEngine;

namespace Framework.UI.Buttons
{
    internal static class CanvasGroupInteraction
    {
        /// <summary>
        /// Same rule as uGUI selectables: a non-interactable CanvasGroup above the object blocks it,
        /// until a group that ignores its parents.
        /// </summary>
        public static bool IsAllowed(Transform target)
        {
            for (var current = target; current != null; current = current.parent)
            {
                if (!current.TryGetComponent(out CanvasGroup group) || !group.enabled)
                {
                    continue;
                }

                if (!group.interactable)
                {
                    return false;
                }

                if (group.ignoreParentGroups)
                {
                    return true;
                }
            }

            return true;
        }
    }
}
