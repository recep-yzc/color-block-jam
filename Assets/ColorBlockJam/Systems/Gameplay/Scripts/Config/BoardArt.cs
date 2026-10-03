using System;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    [CreateAssetMenu(menuName = "Color Block Jam/Gameplay/Board Art", fileName = "BoardArt")]
    public sealed class BoardArt : ScriptableObject
    {
        [Header("Block modules")]
        [Tooltip("Dört yanında da komşusu olan çeyrek blok parçası.")]
        [SerializeField] private Mesh blockCenter;
        [Tooltip("+Y yönünde yan duvarı olan çeyrek blok parçası.")]
        [SerializeField] private Mesh blockEdge;
        [Tooltip("+X +Y yönüne yuvarlanmış dış köşe çeyrek parçası.")]
        [SerializeField] private Mesh blockOuterCorner;
        [Tooltip("İçbükey bir köşeyi saran üç çeyrek. Boş kalan çeyrek +X +Y tarafındadır.")]
        [SerializeField] private Mesh blockInnerCorner;
        [Tooltip("Modüllerin tabanının orijinlerinin ne kadar altında kaldığı, model biriminde.")]
        [SerializeField] private float blockBaseDepth = 0.66f;

        [Header("Arrow blocks")]
        [Tooltip("Ok bloklarının üstüne yatırılan çift uçlu oklar: bir, iki ve üç hücre uzunluğunda.")]
        [SerializeField] private Mesh[] arrows = new Mesh[3];
        [Tooltip("Okun tabanının bloğun altından yüksekliği, sanat biriminde. Bloğun üst yüzünün biraz altında " +
                 "kalır, böylece ok bloğa gömülü görünür.")]
        [SerializeField] private float arrowHeight = 1.45f;
        [Tooltip("Okun renginin, bloğun renginden karıştırılarak yaklaştırıldığı renk.")]
        [SerializeField] private Color arrowColor = new(1f, 0.95f, 0.88f);
        [Tooltip("Okun rengi bloğun renginden ok rengine ne kadar yaklaşır. 0 = bloğun rengi, 1 = ok rengi.")]
        [SerializeField, Range(0f, 1f)] private float arrowColorBlend = 0.75f;

        [Header("Board")]
        [Tooltip("Tahtanın her hücresine döşenen zemin karosu.")]
        [SerializeField] private Mesh groundTile;
        [Tooltip("Kenar boyunca tek parça olarak uzatılan duvar.")]
        [SerializeField] private Mesh wall;
        [Tooltip("Tahtanın dört köşesindeki duvar parçası.")]
        [SerializeField] private Mesh wallCorner;
        [Tooltip("Bir hücre uzunluğundaki kapı parçası. Yan yana aynı renkli kapılar için uzatılır.")]
        [SerializeField] private Mesh door;
        [Tooltip("Kapının ortasına yatırılan ok. Bloğun kapıdan çıkacağı yönü gösterir.")]
        [SerializeField] private Mesh doorArrow;
        [Tooltip("Kapı okunun büyüklüğü, modelin kendi boyutuna göre.")]
        [SerializeField] private float doorArrowScale = 1.4f;
        [Tooltip("Kapı okunun ortasının kapının üst yüzünden yüksekliği, sanat biriminde.")]
        [SerializeField] private float doorArrowLift = 0.12f;
        [Tooltip("Duvar orijininin yüksekliği, sanat biriminde (bir hücre 2 birim). Duvarlar bloklardan uzundur.")]
        [SerializeField] private float wallHeightOffset = -0.8f;
        [Tooltip("Karoların altına serilen düz tabanın yüksekliği, sanat biriminde. Taban sadece oyun alanının " +
                 "hücrelerini kaplar, deliklerde yoktur; karoların arasından görünür.")]
        [SerializeField] private float floorHeight = -0.3f;

        [Header("Model turns")]
        [Tooltip("Duvar modelini ortası etrafında düzelten dönüş, derece. WallAndDoor.fbx içindeki modeller ters " +
                 "dışa aktarılmış, duvar ayrıca çeyrek tur dönük.")]
        [SerializeField] private Vector3 wallModelRotation = new(0f, 90f, 180f);
        [Tooltip("Köşe duvar modelini ortası etrafında düzelten dönüş, derece.")]
        [SerializeField] private Vector3 cornerModelRotation = new(0f, 0f, 180f);
        [Tooltip("Kapı modelini ortası etrafında düzelten dönüş, derece. Kapı X boyunca bir hücre uzunluğunda; " +
                 "çeyrek tur onu kenara dik hale getirirdi.")]
        [SerializeField] private Vector3 doorModelRotation = new(0f, 0f, 180f);
        [Tooltip("Kapı oku modelini düzelten dönüş, derece. Düzeltilmiş ok +Z yönünü göstermeli; tahtanın üst " +
                 "kenarındaki kapının oku tahtanın dışını gösterir.")]
        [SerializeField] private Vector3 doorArrowModelRotation = new(0f, 0f, 180f);

        [Header("Materials")]
        [Tooltip("Bütün blokların paylaştığı materyal. Rengi mesh'in vertex'lerinden gelir.")]
        [SerializeField] private Material blockMaterial;
        [Tooltip("Bütün kapıların paylaştığı materyal. Rengi mesh'in vertex'lerinden gelir.")]
        [SerializeField] private Material doorMaterial;
        [Tooltip("Duvarların materyali.")]
        [SerializeField] private Material wallMaterial;
        [Tooltip("Zemin karolarının materyali.")]
        [SerializeField] private Material groundMaterial;
        [Tooltip("Karoların altındaki düz tabanın materyali.")]
        [SerializeField] private Material floorMaterial;
        [Tooltip("Tutulan bloğun silüetini işaretler, böylece çerçevesi sadece dışına çizilir.")]
        [SerializeField] private Material blockOutlineMaskMaterial;
        [Tooltip("Oyuncunun tuttuğu bloğun etrafına, her şeyin üstünde çizilen dış çerçeve.")]
        [SerializeField] private Material blockOutlineMaterial;

        public Mesh BlockCenter => blockCenter;
        public Mesh BlockEdge => blockEdge;
        public Mesh BlockOuterCorner => blockOuterCorner;
        public Mesh BlockInnerCorner => blockInnerCorner;
        public float BlockBaseDepth => blockBaseDepth;
        public float ArrowHeight => arrowHeight;
        public Mesh GroundTile => groundTile;
        public Mesh Wall => wall;
        public Mesh WallCorner => wallCorner;
        public Mesh Door => door;
        public Mesh DoorArrow => doorArrow;
        public float DoorArrowScale => doorArrowScale;
        public float DoorArrowLift => doorArrowLift;
        public float WallHeightOffset => wallHeightOffset;
        public float FloorHeight => floorHeight;
        public Quaternion WallModelRotation => Quaternion.Euler(wallModelRotation);
        public Quaternion CornerModelRotation => Quaternion.Euler(cornerModelRotation);
        public Quaternion DoorModelRotation => Quaternion.Euler(doorModelRotation);
        public Quaternion DoorArrowModelRotation => Quaternion.Euler(doorArrowModelRotation);
        public Material BlockMaterial => blockMaterial;
        public Material DoorMaterial => doorMaterial;
        public Material WallMaterial => wallMaterial;
        public Material GroundMaterial => groundMaterial;
        public Material FloorMaterial => floorMaterial;
        public Material BlockOutlineMaskMaterial => blockOutlineMaskMaterial;
        public Material BlockOutlineMaterial => blockOutlineMaterial;

        public Mesh ArrowFor(int cells)
        {
            return arrows[Math.Clamp(cells, 1, arrows.Length) - 1];
        }

        public Color ArrowColorOn(Color blockColor)
        {
            return Color.Lerp(blockColor, arrowColor, arrowColorBlend);
        }
    }
}
