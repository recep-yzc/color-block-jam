using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The ground, the walls around the board and the colored doors, each merged into one mesh per material.
    /// Also converts between board cells and world positions: cell (0, 0) starts at this transform's position
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

            AddPart("Ground", ground, art.GroundMaterial);

            var walls = new List<CombineInstance>();
            var doorPieces = new Dictionary<BoardDoor, List<CombineInstance>>();
            BuildSide(board, BoardSide.Bottom, board.Width, art, walls, doorPieces);
            BuildSide(board, BoardSide.Top, board.Width, art, walls, doorPieces);
            BuildSide(board, BoardSide.Left, board.Height, art, walls, doorPieces);
            BuildSide(board, BoardSide.Right, board.Height, art, walls, doorPieces);
            BuildCorners(board, art, walls);
            AddPart("Walls", walls, art.WallMaterial);

            foreach (var pair in doorPieces)
            {
                AddPart($"Door {pair.Key.Side} {pair.Key.Start}", pair.Value, doorMaterials.Get(pair.Key.Color));
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

            for (var i = 0; i < length; i++)
            {
                var door = DoorAt(board, side, i);
                if (door != null)
                {
                    if (!doorPieces.TryGetValue(door, out var pieces))
                    {
                        pieces = new List<CombineInstance>();
                        doorPieces.Add(door, pieces);
                    }

                    pieces.Add(WallPiece(art, art.Door, EdgePoint(board, side, i + 0.5f), turn));
                    continue;
                }

                // A cell edge is two wall pieces long.
                walls.Add(WallPiece(art, art.Wall, EdgePoint(board, side, i + 0.25f), turn));
                walls.Add(WallPiece(art, art.Wall, EdgePoint(board, side, i + 0.75f), turn));
            }
        }

        private void BuildCorners(Board board, BoardArt art, List<CombineInstance> walls)
        {
            const float outside = -0.25f;
            var right = board.Width + 0.25f;
            var top = board.Height + 0.25f;

            walls.Add(WallPiece(art, art.WallCorner, new Vector2(outside, outside), Quaternion.identity));
            walls.Add(WallPiece(art, art.WallCorner, new Vector2(outside, top), Quaternion.Euler(0f, 90f, 0f)));
            walls.Add(WallPiece(art, art.WallCorner, new Vector2(right, top), Quaternion.Euler(0f, 180f, 0f)));
            walls.Add(WallPiece(art, art.WallCorner, new Vector2(right, outside), Quaternion.Euler(0f, 270f, 0f)));
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

        /// <summary>A wall, wall corner or door piece at wall height, turned upright when the art needs it.</summary>
        private CombineInstance WallPiece(BoardArt art, Mesh mesh, Vector2 cell, Quaternion turn)
        {
            var piece = Piece(mesh, cell, art.WallHeightOffset, turn);
            if (art.WallsUpsideDown)
            {
                piece.transform *= UpsideDown(mesh);
            }

            return piece;
        }

        /// <summary>
        /// Turns a model upside down around the middle of its bounds, so it keeps its footprint and height.
        /// The turn is around the Z axis, so the side facing the board still faces it.
        /// </summary>
        private static Matrix4x4 UpsideDown(Mesh mesh)
        {
            var center = mesh.bounds.center;
            return Matrix4x4.TRS(new Vector3(center.x * 2f, center.y * 2f, 0f), Quaternion.Euler(0f, 0f, 180f), Vector3.one);
        }

        private void AddPart(string partName, List<CombineInstance> pieces, Material material)
        {
            if (pieces.Count == 0)
            {
                return;
            }

            var mesh = new Mesh { name = partName, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(pieces.ToArray(), mergeSubMeshes: true, useMatrices: true);
            builtMeshes.Add(mesh);

            var part = new GameObject(partName);
            part.transform.SetParent(transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
