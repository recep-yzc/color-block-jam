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

        private int currentLevel;
        private bool isStarted;

        public ActionButton PlayButton => playButton;

        private void Start()
        {
            isStarted = true;
            ShowTiles();
        }

        public void SetLevel(int level)
        {
            currentLevel = level;
            levelLabel.SetText("Level {0}", level);

            // Layout groups only work after OnEnable, so tiles are measured from Start on.
            if (isStarted)
            {
                ShowTiles();
            }
        }

        private void ShowTiles()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(pathNodesRoot);

            var nodes = pathNodesRoot.GetComponentsInChildren<LevelPathNodeView>(true);
            Array.Sort(nodes, (a, b) => HeightInRoot(a).CompareTo(HeightInRoot(b)));

            // The lowest tile is the current level; the ones above it are not reached yet.
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].Show(currentLevel + i, isLocked: i > 0);
            }
        }

        private float HeightInRoot(Component node)
        {
            return pathNodesRoot.InverseTransformPoint(node.transform.position).y;
        }
    }
}
