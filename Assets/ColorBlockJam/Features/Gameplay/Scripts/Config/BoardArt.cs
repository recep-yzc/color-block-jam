using System;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The meshes and materials the board is built from. Block modules and arrows are modeled on the XY plane
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

        [Header("Arrow blocks")]
        [Tooltip("Double-headed arrows laid along an arrow block, one, two and three cells long.")]
        [SerializeField] private Mesh[] arrows = new Mesh[3];
        [Tooltip("Height of the arrow's base above the block's bottom, in art units. A little under the block's top, " +
                 "so the arrow looks pressed into it.")]
        [SerializeField] private float arrowHeight = 1.45f;
        [Tooltip("Color the arrow is blended toward from its block's color.")]
        [SerializeField] private Color arrowColor = new(1f, 0.95f, 0.88f);
        [Tooltip("How far the arrow's color goes from the block's color toward the arrow color.")]
        [SerializeField, Range(0f, 1f)] private float arrowColorBlend = 0.75f;

        [Header("Board")]
        [SerializeField] private Mesh groundTile;
        [SerializeField] private Mesh wall;
        [SerializeField] private Mesh wallCorner;
        [Tooltip("Door piece one cell long.")]
        [SerializeField] private Mesh door;
        [Tooltip("Height of the wall origin, in art units (a cell is 2). Walls are taller than blocks.")]
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
        [Tooltip("Marks the held block's silhouette, so its outline draws only outside it.")]
        [SerializeField] private Material blockOutlineMaskMaterial;
        [Tooltip("The outer rim drawn around the block the player holds, on top of everything.")]
        [SerializeField] private Material blockOutlineMaterial;
        [Tooltip("See-through shell around a frozen block.")]
        [SerializeField] private Material iceMaterial;

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
        public float WallHeightOffset => wallHeightOffset;
        public Quaternion WallModelRotation => Quaternion.Euler(wallModelRotation);
        public Quaternion CornerModelRotation => Quaternion.Euler(cornerModelRotation);
        public Quaternion DoorModelRotation => Quaternion.Euler(doorModelRotation);
        public Material BlockMaterial => blockMaterial;
        public Material DoorMaterial => doorMaterial;
        public Material WallMaterial => wallMaterial;
        public Material GroundMaterial => groundMaterial;
        public Material BlockOutlineMaskMaterial => blockOutlineMaskMaterial;
        public Material BlockOutlineMaterial => blockOutlineMaterial;
        public Material IceMaterial => iceMaterial;

        /// <summary>The arrow for a straight run of <paramref name="cells"/> cells; longer runs get the longest arrow.</summary>
        public Mesh ArrowFor(int cells)
        {
            return arrows[Math.Clamp(cells, 1, arrows.Length) - 1];
        }

        /// <summary>The arrow's color on a block of <paramref name="blockColor"/>: a light shade of it.</summary>
        public Color ArrowColorOn(Color blockColor)
        {
            return Color.Lerp(blockColor, arrowColor, arrowColorBlend);
        }
    }
}
