using System;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    [Serializable]
    public struct FailReasonDisplay
    {
        [Tooltip("Bu yazının ve ikonun gösterildiği kaybetme nedeni.")]
        public LevelFailReason reason;
        [Tooltip("Popup'ta gösterilen yazı.")]
        public string text;
        [Tooltip("Popup'ta gösterilen ikon.")]
        public Sprite icon;
    }
}
