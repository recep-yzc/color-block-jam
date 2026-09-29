using TMPro;
using UnityEngine;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// One level tile on the home page path. Its look (current or locked) is set in the prefab.
    /// </summary>
    public sealed class LevelPathNodeView : MonoBehaviour
    {
        [SerializeField] private TMP_Text numberLabel;

        public void SetLevel(int level)
        {
            numberLabel.SetText("{0}", level);
        }
    }
}
