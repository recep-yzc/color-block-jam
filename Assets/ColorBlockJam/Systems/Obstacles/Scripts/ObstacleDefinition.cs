using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Obstacles
{
    public abstract class ObstacleDefinition : ScriptableObject
    {
        [Tooltip("Engelin kayıtlardaki kimliği. Oyuncunun engeli görüp görmediği buna bağlı tutulur, sonradan değiştirilmemeli.")]
        [SerializeField] private string id;
        [Tooltip("Tanıtım popup'ında görünen adı.")]
        [SerializeField] private string displayName;
        [Tooltip("Tanıtım popup'ında engelin nasıl çalıştığını anlatan yazı.")]
        [SerializeField, TextArea] private string description;
        [Tooltip("Tanıtım popup'ında görünen ikon. Boş bırakılırsa ikon gizlenir.")]
        [SerializeField] private Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public abstract bool AppearsIn(LevelData level);
    }
}
