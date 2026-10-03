using ColorBlockJam.Gameplay.Logic;
using LitMotion;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    [CreateAssetMenu(menuName = "Color Block Jam/Gameplay/Gameplay Config", fileName = "GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Board")]
        [Tooltip("Bir tahta hücresinin dünya boyutu. Sanat 2 birimlik hücre için yapıldı.")]
        [SerializeField, Min(0.1f)] private float cellSize = 2f;

        [Header("Drag")]
        [Tooltip("Blokların etrafındaki dokunma payı, hücre. Boş bir hücreye basıldığında bu pay içinde bir blok varsa " +
                 "en yakını tutulur. 0 = sadece bloğun üstü.")]
        [SerializeField, Range(0f, 0.5f)] private float pickPadding = 0.3f;
        [Tooltip("Sürüklenen bloğun parmağa ne kadar hızlı yetiştiği. Yüksek değer daha sıkı, düşük değer daha ağır " +
                 "hissettirir.")]
        [SerializeField, Min(1f)] private float followSharpness = 26f;
        [Tooltip("Sürüklenen bloğun en yüksek hızı, saniyede hücre. Hızlı bir savuruş kontrolden çıkmasın diye.")]
        [SerializeField, Min(1f)] private float maxDragSpeed = 40f;
        [Tooltip("Blok köşelerindeki görünmez yuvarlamanın yarıçapı, hücre. Blok köşeye takılmak yerine bu eğri " +
                 "üzerinden döner. 0 = keskin köşeler.")]
        [SerializeField, Range(0f, 0.45f)] private float cornerRounding = 0.3f;
        [Tooltip("Sürüklenen bloğun ne kadar yükseldiği, dünya biriminde.")]
        [SerializeField, Min(0f)] private float liftHeight = 0.35f;
        [Tooltip("Bloğun kalkma ve inme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float liftDuration = 0.1f;
        [Tooltip("Bırakılan bloğun en yakın boş hücreye oturma süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float snapDuration = 0.14f;
        [Tooltip("Oturma hareketinin eğrisi. Hedefi aşan bir eğri bloğu bir anlığına komşusunun içine iterdi.")]
        [SerializeField] private Ease snapEase = Ease.OutCubic;

        [Header("Exit")]
        [Tooltip("Bloğun çıkması için kapısından kaç hücre içeri itilmesi gerektiği.")]
        [SerializeField, Range(0.05f, 1f)] private float exitDepth = 0.3f;
        [Tooltip("Çıkan bloğun hızı, saniyede hücre.")]
        [SerializeField, Min(0.1f)] private float exitSpeed = 12f;
        [Tooltip("Çıkan bloğun gizlenmeden önce tahtanın ne kadar dışına kaydığı, hücre.")]
        [SerializeField, Min(0f)] private float exitOvershoot = 0.5f;
        [Tooltip("Kapı önüne bırakılan bloğun kapıya hizalanırken oturma süresinin ne kadarını kullandığı.")]
        [SerializeField, Range(0.1f, 1f)] private float lineUpShare = 0.6f;
        [Tooltip("Çıkan bloğun kaybolurken küçüldüğü ölçek, kendi boyutuna oranla.")]
        [SerializeField, Range(0f, 1f)] private float exitShrink = 0.2f;
        [Tooltip("Blok geçerken kapının basıldığı yükseklik, kendi yüksekliğine oranla.")]
        [SerializeField, Range(0.05f, 1f)] private float doorOpenSquash = 0.35f;
        [Tooltip("Kapının basılma süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float doorOpenDuration = 0.08f;
        [Tooltip("Blok geçsin diye kapının açık kaldığı süre, saniye.")]
        [SerializeField, Min(0f)] private float doorHoldDuration = 0.18f;
        [Tooltip("Kapının yaylanarak geri kalkma süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float doorCloseDuration = 0.35f;
        [Tooltip("Blok tahtadan çıkınca oynayan patlama efekti. Bloğun rengini alır ve havuzdan gelir.")]
        [SerializeField] private ParticleSystem burstPrefab;
        [Tooltip("Seviye açılırken havuzda hazır bekletilen patlama sayısı.")]
        [SerializeField, Min(0)] private int burstPrewarm = 4;
        [Tooltip("Seviye bittikten sonra sonuç popup'ı açılana kadar geçen süre, saniye. Son hareketler bitsin diye.")]
        [SerializeField, Min(0f)] private float resultPopupDelay = 0.6f;

        [Header("Ice")]
        [Tooltip("Yeterince blok çıkınca buzun kırılıp kaybolma süresi, ayrıca sayının zıplama süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float iceBreakDuration = 0.3f;
        [Tooltip("Buz sayısı azalınca bir anlığına ne kadar büyüdüğü.")]
        [SerializeField, Min(0f)] private float iceCountPunch = 0.35f;
        [Tooltip("Buz kırılınca oynayan patlamanın rengi.")]
        [SerializeField] private Color iceBurstColor = new(0.72f, 0.9f, 1f);
        [Tooltip("Donmuş bir blok çekilmeye çalışılınca ne kadar sallandığı, hücre.")]
        [SerializeField, Min(0f)] private float frozenShakeStrength = 0.08f;
        [Tooltip("Donmuş bloğun sallanma süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float frozenShakeDuration = 0.3f;
        [Tooltip("Donmuş bloğun sallanırken kaç kez gidip geldiği.")]
        [SerializeField, Min(1)] private int frozenShakeFrequency = 6;
        [Tooltip("Buz sayısının bloğun üst yüzünden yüksekliği, sanat biriminde (bir hücre 2 birim).")]
        [SerializeField, Min(0f)] private float iceCountLift = 0.25f;

        [Header("Breaking")]
        [Tooltip("Bir booster'ın kırdığı bloğun patlamadan önce ezilme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float smashDuration = 0.2f;
        [Tooltip("Ezilen bloğun yanlara yayılması, kendi genişliğine oranla.")]
        [SerializeField, Min(1f)] private float smashSpread = 1.2f;
        [Tooltip("Ezilen bloğun patlamadan önceki yüksekliği, kendi yüksekliğine oranla.")]
        [SerializeField, Range(0.05f, 1f)] private float smashHeight = 0.3f;

        [Header("Solver")]
        [Tooltip("Tahtanın hâlâ temizlenebilir olup olmadığını anlamak için aranan en fazla durum sayısı.")]
        [SerializeField, Min(100)] private int stuckSearchBudget = BoardSolver.DefaultBudget;
        [Tooltip("Otomatik oynatmanın çözümü bulmak için aradığı en fazla durum sayısı.")]
        [SerializeField, Min(100)] private int autoPlaySearchBudget = 60000;
        [Tooltip("Otomatik oynatmada bloğun bir hücre kayma süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float autoPlayCellDuration = 0.08f;
        [Tooltip("Otomatik oynatmada iki hamle arasındaki bekleme, saniye.")]
        [SerializeField, Min(0f)] private float autoPlayPause = 0.12f;

        [Header("Timer")]
        [Tooltip("Süre bu kadar saniyenin altına inince sayaç uyarı görünümüne geçer.")]
        [SerializeField, Min(0)] private int timerWarningSeconds = 10;

        [Header("Out Of Time")]
        [Tooltip("Süre bitince coin karşılığında eklenen süre, saniye.")]
        [SerializeField, Min(1)] private int extraTimeSeconds = 20;
        [Tooltip("Ek sürenin coin bedeli.")]
        [SerializeField, Min(1)] private int extraTimeCost = 100;

        [Header("Camera")]
        [Tooltip("Kameranın eğim açısı, derece. 90 tam yukarıdan bakar.")]
        [SerializeField, Range(30f, 90f)] private float cameraPitch = 62f;
        [Tooltip("Tahtanın kullanabileceği ekran yüksekliği oranı. Kalanı HUD'a kalır.")]
        [SerializeField, Range(0.3f, 1f)] private float boardScreenHeight = 0.62f;
        [Tooltip("Tahtanın kullanabileceği ekran genişliği oranı.")]
        [SerializeField, Range(0.3f, 1f)] private float boardScreenWidth = 0.92f;
        [Tooltip("Tahtanın arkasındaki renk.")]
        [SerializeField] private Color backgroundColor = new(0.4863f, 0.5451f, 0.902f);

        [Header("Views")]
        [Tooltip("Her blok için oluşturulan görünüm prefab'ı.")]
        [SerializeField] private BlockView blockViewPrefab;

        public float CellSize => cellSize;
        public float PickPadding => pickPadding;
        public float FollowSharpness => followSharpness;
        public float MaxDragSpeed => maxDragSpeed;
        public float CornerRounding => cornerRounding;
        public float LiftHeight => liftHeight;
        public float LiftDuration => liftDuration;
        public float SnapDuration => snapDuration;
        public Ease SnapEase => snapEase;
        public float ExitDepth => exitDepth;
        public float ExitSpeed => exitSpeed;
        public float ExitOvershoot => exitOvershoot;
        public float LineUpShare => lineUpShare;
        public float ExitShrink => exitShrink;
        public float DoorOpenSquash => doorOpenSquash;
        public float DoorOpenDuration => doorOpenDuration;
        public float DoorHoldDuration => doorHoldDuration;
        public float DoorCloseDuration => doorCloseDuration;
        public ParticleSystem BurstPrefab => burstPrefab;
        public int BurstPrewarm => burstPrewarm;
        public float ResultPopupDelay => resultPopupDelay;
        public float IceBreakDuration => iceBreakDuration;
        public float IceCountPunch => iceCountPunch;
        public Color IceBurstColor => iceBurstColor;
        public float FrozenShakeStrength => frozenShakeStrength;
        public float FrozenShakeDuration => frozenShakeDuration;
        public int FrozenShakeFrequency => frozenShakeFrequency;
        public float IceCountLift => iceCountLift;
        public float SmashDuration => smashDuration;
        public float SmashSpread => smashSpread;
        public float SmashHeight => smashHeight;
        public int StuckSearchBudget => stuckSearchBudget;
        public int AutoPlaySearchBudget => autoPlaySearchBudget;
        public float AutoPlayCellDuration => autoPlayCellDuration;
        public float AutoPlayPause => autoPlayPause;
        public int TimerWarningSeconds => timerWarningSeconds;
        public int ExtraTimeSeconds => extraTimeSeconds;
        public int ExtraTimeCost => extraTimeCost;
        public float CameraPitch => cameraPitch;
        public float BoardScreenHeight => boardScreenHeight;
        public float BoardScreenWidth => boardScreenWidth;
        public Color BackgroundColor => backgroundColor;
        public BlockView BlockViewPrefab => blockViewPrefab;
    }
}
