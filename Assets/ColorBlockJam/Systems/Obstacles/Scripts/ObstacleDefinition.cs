using UnityEngine;

namespace ColorBlockJam.Obstacles
{
    [CreateAssetMenu(menuName = "Color Block Jam/Obstacles/Obstacle", fileName = "Obstacle")]
    public sealed class ObstacleDefinition : ScriptableObject
    {
        [Tooltip("Engelin kayıtlardaki kimliği. Oyuncunun engeli görüp görmediği buna bağlı tutulur, sonradan değiştirilmemeli.")]
        [SerializeField] private string id;
        [Tooltip("Engelin türü. Bir seviyede bu tür ilk kez görüldüğünde tanıtım popup'ı çıkar.")]
        [SerializeField] private ObstacleKind kind;
        [Tooltip("Tanıtım popup'ında görünen adı.")]
        [SerializeField] private string displayName;
        [Tooltip("Tanıtım popup'ında engelin nasıl çalıştığını anlatan yazı.")]
        [SerializeField, TextArea] private string description;
        [Tooltip("Tanıtım popup'ında görünen ikon. Boş bırakılırsa ikon gizlenir.")]
        [SerializeField] private Sprite icon;

        public string Id => id;
        public ObstacleKind Kind => kind;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
    }
}
