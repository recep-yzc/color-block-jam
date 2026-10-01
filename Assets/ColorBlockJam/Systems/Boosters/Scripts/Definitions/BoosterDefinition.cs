using UnityEngine;

namespace ColorBlockJam.Boosters
{
    public abstract class BoosterDefinition : ScriptableObject
    {
        [Tooltip("Booster'ın kayıtlardaki kimliği. Oyuncunun adetleri buna bağlı tutulur, sonradan değiştirilmemeli.")]
        [SerializeField] private string id;
        [Tooltip("Açılış popup'ında görünen adı.")]
        [SerializeField] private string displayName;
        [Tooltip("Açılış popup'ında booster'ın ne yaptığını anlatan yazı.")]
        [SerializeField, TextArea] private string description;
        [Tooltip("Butonda ve açılış popup'ında görünen ikon.")]
        [SerializeField] private Sprite icon;
        [Tooltip("Booster'ın açıldığı seviye. O seviyenin başında açılış popup'ı çıkar.")]
        [SerializeField, Min(1)] private int unlockLevel = 1;
        [Tooltip("Booster açılınca oyuncuya verilen adet.")]
        [SerializeField, Min(0)] private int startingCount = 1;
        [Tooltip("Elde hiç kalmadığında bir kullanımın coin fiyatı.")]
        [SerializeField, Min(1)] private int coinCost = 50;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int UnlockLevel => unlockLevel;
        public int StartingCount => startingCount;
        public int CoinCost => coinCost;

        public abstract BoosterEffect CreateEffect(BoosterContext context);
    }
}
