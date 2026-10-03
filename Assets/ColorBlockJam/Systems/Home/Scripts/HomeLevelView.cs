using System;
using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Home
{
    public sealed class HomeLevelView : UIView
    {
        [Tooltip("Sıradaki seviyeyi başlatan buton.")]
        [SerializeField] private ActionButton playButton;
        [Tooltip("Sıradaki seviyenin numarasını gösteren yazı.")]
        [SerializeField] private TMP_Text levelLabel;
        [Tooltip("Seviye karolarının altında durduğu obje. En alttaki karo şu anki seviyeyi, üstündekiler sonraki " +
                 "seviyeleri gösterir.")]
        [SerializeField] private RectTransform pathNodesRoot;

        private int currentLevel;
        private bool isStarted;

        public ActionButton PlayButton => playButton;

        private void Start()
        {
            isStarted = true;
            ShowTiles();
        }

        public void Show(int level)
        {
            currentLevel = level;
            levelLabel.SetText("Level {0}", level);

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

            for (var i = 0; i < nodes.Length; i++)
            {
                var level = currentLevel + i;
                nodes[i].Show(level, isLocked: i > 0);
            }
        }

        private float HeightInRoot(Component node)
        {
            return pathNodesRoot.InverseTransformPoint(node.transform.position).y;
        }
    }
}
