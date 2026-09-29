using System;
using Framework.UI.Buttons;
using Framework.UI.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The level button and the level path of the home page.
    /// </summary>
    public sealed class HomeLevelView : UIView
    {
        [SerializeField] private ActionButton playButton;
        [SerializeField] private TMP_Text levelLabel;
        [Tooltip("Parent of the level tiles. The lowest tile shows the current level, the ones above it the next levels.")]
        [SerializeField] private RectTransform pathNodesRoot;

        public ActionButton PlayButton => playButton;

        public void SetLevel(int level)
        {
            levelLabel.SetText("Level {0}", level);

            // Presenters run before the first layout pass, so place the tiles before measuring them.
            LayoutRebuilder.ForceRebuildLayoutImmediate(pathNodesRoot);

            var nodes = pathNodesRoot.GetComponentsInChildren<LevelPathNodeView>(true);
            Array.Sort(nodes, (a, b) => HeightInRoot(a).CompareTo(HeightInRoot(b)));

            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].SetLevel(level + i);
            }
        }

        private float HeightInRoot(Component node)
        {
            return pathNodesRoot.InverseTransformPoint(node.transform.position).y;
        }
    }
}
