using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The meshes and materials the board is built from. Block modules are quarter cells modeled on the XY plane
    /// facing -Z; walls, doors and ground tiles are modeled Y-up. All are made for a 2-unit cell.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Gameplay/Board Art", fileName = "BoardArt")]
    public sealed class BoardArt : ScriptableObject
    {
        [Header("Block modules")]
        [Tooltip("Quarter with neighbors on every side.")]
        [SerializeField] private Mesh blockCenter;
        [Tooltip("Quarter with a side wall toward +Y.")]
        [SerializeField] private Mesh blockEdge;
        [Tooltip("Quarter rounded toward +X +Y.")]
        [SerializeField] private Mesh blockOuterCorner;
        [Tooltip("Three quarters around a concave corner; the open quarter is +X +Y.")]
        [SerializeField] private Mesh blockInnerCorner;
        [Tooltip("Height of the modules' base below their origin, in model units.")]
        [SerializeField] private float blockBaseDepth = 0.66f;

        [Header("Board")]
        [SerializeField] private Mesh groundTile;
        [SerializeField] private Mesh wall;
        [SerializeField] private Mesh wallCorner;
        [Tooltip("Door piece one cell long.")]
        [SerializeField] private Mesh door;
        [Tooltip("Height of the wall origin, in world units. Walls are taller than blocks.")]
        [SerializeField] private float wallHeightOffset = -0.8f;

        [Header("Model turns")]
        [Tooltip("Turn, in degrees, that sets the wall model right, around its middle. The models in WallAndDoor.fbx " +
                 "are exported upside down, and the wall also turned a quarter.")]
        [SerializeField] private Vector3 wallModelRotation = new(0f, 90f, 180f);
        [Tooltip("Turn, in degrees, that sets the wall corner model right, around its middle.")]
        [SerializeField] private Vector3 cornerModelRotation = new(0f, 0f, 180f);
        [Tooltip("Turn, in degrees, that sets the door model right, around its middle. The door is one cell long " +
                 "along X, so a quarter turn would stand it across the edge.")]
        [SerializeField] private Vector3 doorModelRotation = new(0f, 0f, 180f);

        [Header("Materials")]
        [Tooltip("Tinted with the palette color of each block.")]
        [SerializeField] private Material blockMaterial;
        [Tooltip("Tinted with the palette color of each door.")]
        [SerializeField] private Material doorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material groundMaterial;
        [Tooltip("Drawn as a second material on the block the player holds, as its outline.")]
        [SerializeField] private Material blockOutlineMaterial;

        public Mesh BlockCenter => blockCenter;
        public Mesh BlockEdge => blockEdge;
        public Mesh BlockOuterCorner => blockOuterCorner;
        public Mesh BlockInnerCorner => blockInnerCorner;
        public float BlockBaseDepth => blockBaseDepth;
        public Mesh GroundTile => groundTile;
        public Mesh Wall => wall;
        public Mesh WallCorner => wallCorner;
        public Mesh Door => door;
        public float WallHeightOffset => wallHeightOffset;
        public Quaternion WallModelRotation => Quaternion.Euler(wallModelRotation);
        public Quaternion CornerModelRotation => Quaternion.Euler(cornerModelRotation);
        public Quaternion DoorModelRotation => Quaternion.Euler(doorModelRotation);
        public Material BlockMaterial => blockMaterial;
        public Material DoorMaterial => doorMaterial;
        public Material WallMaterial => wallMaterial;
        public Material GroundMaterial => groundMaterial;
        public Material BlockOutlineMaterial => blockOutlineMaterial;
    }
}
