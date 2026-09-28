using TMPro;
using UnityEngine;

namespace ColorBlockJam.Boot
{
    public sealed class LoadingBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform fill;
        [SerializeField] private TMP_Text percentLabel;

        public void SetProgress(float normalizedProgress)
        {
            var progress = Mathf.Clamp01(normalizedProgress);
            fill.anchorMax = new Vector2(progress, fill.anchorMax.y);
            percentLabel.SetText("{0}%", Mathf.RoundToInt(progress * 100f));
        }
    }
}
