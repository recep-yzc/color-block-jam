using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The board as one mesh: ground tiles, walls and corners, with a submesh for the ground and one for the walls.
    /// Each colored door is a mesh of its own. A wall between two doors or corners is a single wall piece stretched
    /// to fit. Also converts between board cells and world positions: cell (0, 0) starts at this transform's position
    /// and cells grow along world X and Z.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<Mesh> builtMeshes = new();
        private float cellSize;
        private float artScale;

        public Bounds WorldBounds { get; private set; }

        public void Build(Board board, BoardArt art, ColorMaterials doorMaterials, float worldCellSize)
        {
            cellSize = worldCellSize;
            artScale = worldCellSize / ArtSpace.CellSize;

            var ground = new List<CombineInstance>();
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    ground.Add(Piece(art.GroundTile, new Vector2(x + 0.5f, y + 0.5f), 0f, Quaternion.identity));
                }
            }

            var walls = new List<CombineInstance>();
            var doorPieces = new Dictionary<BoardDoor, List<CombineInstance>>();
            BuildSide(board, BoardSide.Bottom, board.Width, art, walls, doorPieces);
            BuildSide(board, BoardSide.Top, board.Width, art, walls, doorPieces);
            BuildSide(board, BoardSide.Left, board.Height, art, walls, doorPieces);
            BuildSide(board, BoardSide.Right, board.Height, art, walls, doorPieces);
            BuildCorners(board, art, walls);
            AddBoard(ground, walls, art);

            foreach (var pair in doorPieces)
            {
                var partName = $"Door {pair.Key.Side} {pair.Key.Start}";
                AddRenderer(partName, Combine(partName, pair.Value), doorMaterials.Get(pair.Key.Color));
            }

            var size = new Vector3((board.Width + 1) * cellSize, cellSize, (board.Height + 1) * cellSize);
            WorldBounds = new Bounds(CellToWorld(new Vector2(board.Width * 0.5f, board.Height * 0.5f)), size);
        }

        /// <summary>World position of a point in cell units, on the ground.</summary>
        public Vector3 CellToWorld(Vector2 cell)
        {
            return transform.position + new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
        }

        /// <summary>Where a screen point hits a horizontal plane at <paramref name="height"/>, in cell units.</summary>
        public bool TryScreenToCell(Camera viewCamera, Vector2 screenPoint, float height, out Vector2 cell)
        {
            var ray = viewCamera.ScreenPointToRay(screenPoint);
            var plane = new Plane(Vector3.up, transform.position + Vector3.up * height);
            if (!plane.Raycast(ray, out var distance))
            {
                cell = default;
                return false;
            }

            var local = ray.GetPoint(distance) - transform.position;
            cell = new Vector2(local.x / cellSize, local.z / cellSize);
            return true;
        }

        private void OnDestroy()
        {
            foreach (var mesh in builtMeshes)
            {
                Destroy(mesh);
            }
        }

        private void BuildSide(Board board, BoardSide side, int length, BoardArt art,
            List<CombineInstance> walls, Dictionary<BoardDoor, List<CombineInstance>> doorPieces)
        {
            var turn = side is BoardSide.Left or BoardSide.Right ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            var runStart = 0;

            for (var i = 0; i <= length; i++)
            {
                var door = i < length ? DoorAt(board, side, i) : null;
                if (i < length && door == null)
                {
                    continue;
                }

                // A run of wall ends at a door or at the corner: one wall piece stretched over all of it.
                if (i > runStart)
                {
                    walls.Add(StretchedWall(board, art, side, runStart, i, turn));
                }

                runStart = i + 1;
                if (door == null)
                {
                    continue;
                }

                if (!doorPieces.TryGetValue(door, out var pieces))
                {
                    pieces = new List<CombineInstance>();
                    doorPieces.Add(door, pieces);
                }

                pieces.Add(WallPiece(art, art.Door, art.DoorModelRotation, EdgePoint(board, side, i + 0.5f), turn, stretch: 1f));
            }
        }

        /// <summary>One wall piece stretched along a side from cell <paramref name="from"/> up to <paramref name="to"/>.</summary>
        private CombineInstance StretchedWall(Board board, BoardArt art, BoardSide side, int from, int to, Quaternion turn)
        {
            var mesh = art.Wall;
            var rotation = art.WallModelRotation;
            var stretch = (to - from) * ArtSpace.CellSize / LengthAlongX(mesh, rotation);
            return WallPiece(art, mesh, rotation, EdgePoint(board, side, (from + to) * 0.5f), turn, stretch);
        }

        private void BuildCorners(Board board, BoardArt art, List<CombineInstance> walls)
        {
            const float outside = -0.25f;
            var right = board.Width + 0.25f;
            var top = board.Height + 0.25f;

            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(outside, outside), Quaternion.identity, 1f));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(outside, top), Quaternion.Euler(0f, 90f, 0f), 1f));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(right, top), Quaternion.Euler(0f, 180f, 0f), 1f));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(right, outside), Quaternion.Euler(0f, 270f, 0f), 1f));
        }

        /// <summary>The middle of the wall band outside a side, at a distance along that side, in cell units.</summary>
        private static Vector2 EdgePoint(Board board, BoardSide side, float along)
        {
            // Walls are a quarter cell thick band just outside the board.
            const float band = 0.25f;
            return side switch
            {
                BoardSide.Bottom => new Vector2(along, -band),
                BoardSide.Top => new Vector2(along, board.Height + band),
                BoardSide.Left => new Vector2(-band, along),
                _ => new Vector2(board.Width + band, along)
            };
        }

        private static BoardDoor DoorAt(Board board, BoardSide side, int alongEdge)
        {
            foreach (var door in board.Doors)
            {
                if (door.Side == side && door.Covers(alongEdge))
                {
                    return door;
                }
            }

            return null;
        }

        private CombineInstance Piece(Mesh mesh, Vector2 cell, float height, Quaternion turn)
        {
            var position = new Vector3(cell.x * cellSize, height * artScale, cell.y * cellSize);
            return new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, turn, Vector3.one * artScale) };
        }

        /// <summary>
        /// A wall, wall corner or door piece at wall height: its model set right first, then stretched along the side
        /// by <paramref name="stretch"/> around its middle.
        /// </summary>
        private CombineInstance WallPiece(BoardArt art, Mesh mesh, Quaternion modelRotation, Vector2 cell, Quaternion turn, float stretch)
        {
            var middle = mesh.bounds.center;
            var alongSide = Matrix4x4.TRS(new Vector3(middle.x * (1f - stretch), 0f, 0f), Quaternion.identity, new Vector3(stretch, 1f, 1f));
            var piece = Piece(mesh, cell, art.WallHeightOffset, turn);
            piece.transform *= alongSide * AroundMiddle(mesh, modelRotation);
            return piece;
        }

        /// <summary>Turns a model around the middle of its bounds, so it stays where it was.</summary>
        private static Matrix4x4 AroundMiddle(Mesh mesh, Quaternion rotation)
        {
            var middle = mesh.bounds.center;
            return Matrix4x4.TRS(middle - rotation * middle, rotation, Vector3.one);
        }

        /// <summary>How long the model is along X once turned, in art units.</summary>
        private static float LengthAlongX(Mesh mesh, Quaternion rotation)
        {
            var extents = mesh.bounds.extents;
            var turned = Matrix4x4.Rotate(rotation);
            return 2f * (Mathf.Abs(turned.m00) * extents.x + Mathf.Abs(turned.m01) * extents.y + Mathf.Abs(turned.m02) * extents.z);
        }

        /// <summary>Ground and walls as one mesh with a submesh each, so the whole board is one renderer.</summary>
        private void AddBoard(List<CombineInstance> ground, List<CombineInstance> walls, BoardArt art)
        {
            var groundMesh = Combine("Ground", ground);
            var wallMesh = Combine("Walls", walls);
            var mesh = new Mesh { name = "Board", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(new[] { new CombineInstance { mesh = groundMesh }, new CombineInstance { mesh = wallMesh } },
                mergeSubMeshes: false, useMatrices: false);
            Destroy(groundMesh);
            Destroy(wallMesh);
            AddRenderer("Board", mesh, art.GroundMaterial, art.WallMaterial);
        }

        private static Mesh Combine(string meshName, List<CombineInstance> pieces)
        {
            var mesh = new Mesh { name = meshName, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(pieces.ToArray(), mergeSubMeshes: true, useMatrices: true);
            return mesh;
        }

        private void AddRenderer(string partName, Mesh mesh, params Material[] materials)
        {
            builtMeshes.Add(mesh);
            var part = new GameObject(partName);
            part.transform.SetParent(transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterials = materials;
        }
    }
}
