using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    internal static class CanvasGroupInteraction
    {
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
