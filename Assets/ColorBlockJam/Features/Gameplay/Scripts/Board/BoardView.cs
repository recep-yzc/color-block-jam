using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The board as one mesh: ground tiles, walls and corners, with a submesh for the ground and one for the walls.
    /// Each colored door is a mesh of its own. Along a side, every run of wall, and every run of door cells of one
    /// color, even when it is several doors side by side, is a single piece stretched to fit.
    /// Also converts between board cells and world positions: cell (0, 0) starts at this transform's position
    /// and cells grow along world X and Z.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private const int NoDoor = -1;

        /// <summary>A stretched door piece covering the cells <see cref="From"/> up to <see cref="To"/> of a side, in one color.</summary>
        private readonly struct DoorRun
        {
            public readonly BoardSide Side;
            public readonly int From;
            public readonly int To;
            public readonly int Color;
            public readonly Vector3 Pivot;
            public readonly CombineInstance Piece;

            public DoorRun(BoardSide side, int from, int to, int color, Vector3 pivot, CombineInstance piece)
            {
                Side = side;
                From = from;
                To = to;
                Color = color;
                Pivot = pivot;
                Piece = piece;
            }
        }

        private readonly List<Mesh> builtMeshes = new();
        private readonly List<DoorView> doorViews = new();
        private Transform staticParts;
        private Transform doorParts;
        private float cellSize;
        private float artScale;

        public Bounds WorldBounds { get; private set; }

        public void Build(Board board, BoardArt art, BlockPalette palette, GameplayConfig config)
        {
            cellSize = config.CellSize;
            artScale = cellSize / ArtSpace.CellSize;

            // The board sits apart from the blocks, which are children of this view too, and from the doors: both move.
            staticParts = new GameObject("Board Parts") { isStatic = true }.transform;
            staticParts.SetParent(transform, false);
            doorParts = new GameObject("Doors").transform;
            doorParts.SetParent(transform, false);

            var ground = new List<CombineInstance>();
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    ground.Add(Piece(art.GroundTile, new Vector2(x + 0.5f, y + 0.5f), 0f, Quaternion.identity));
                }
            }

            var walls = new List<CombineInstance>();
            var doors = new List<DoorRun>();
            BuildSide(board, BoardSide.Bottom, board.Width, art, walls, doors);
            BuildSide(board, BoardSide.Top, board.Width, art, walls, doors);
            BuildSide(board, BoardSide.Left, board.Height, art, walls, doors);
            BuildSide(board, BoardSide.Right, board.Height, art, walls, doors);
            BuildCorners(board, art, walls);
            AddBoard(ground, walls, art);

            foreach (var door in doors)
            {
                // Built around the middle of its base, so it can squash toward the ground.
                var piece = door.Piece;
                piece.transform = Matrix4x4.Translate(-door.Pivot) * piece.transform;
                var doorName = $"Door {door.Side} {door.From}";
                var doorRenderer = AddRenderer(doorParts, doorName, Combine(doorName, new List<CombineInstance> { piece }), art.DoorMaterial);
                doorRenderer.transform.localPosition = door.Pivot;
                ToonTint.Apply(doorRenderer, palette.GetColor(door.Color));

                var doorView = doorRenderer.gameObject.AddComponent<DoorView>();
                doorView.Initialize(door.Side, door.From, door.To, config);
                doorViews.Add(doorView);
            }

            // The board itself never moves: it is marked static and batched as static geometry. Static
            // batching in a build only covers objects saved in a scene, so the built board is combined here.
            StaticBatchingUtility.Combine(staticParts.gameObject);

            var size = new Vector3((board.Width + 1) * cellSize, cellSize, (board.Height + 1) * cellSize);
            WorldBounds = new Bounds(CellToWorld(new Vector2(board.Width * 0.5f, board.Height * 0.5f)), size);
        }

        /// <summary>Plays the opening of the door a block is going through.</summary>
        public void PlayDoorEntry(BoardDoor door)
        {
            foreach (var doorView in doorViews)
            {
                if (doorView.Covers(door.Side, door.Start))
                {
                    doorView.PlayEntry();
                    return;
                }
            }
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

        /// <summary>
        /// Walks a side cell by cell and ends a run wherever the door color changes; wall cells count as a color of
        /// their own. Each run becomes one piece stretched over it: a wall, or a door of that color.
        /// </summary>
        private void BuildSide(Board board, BoardSide side, int length, BoardArt art, List<CombineInstance> walls, List<DoorRun> doors)
        {
            var turn = side is BoardSide.Left or BoardSide.Right ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            var runStart = 0;
            var runColor = DoorColorAt(board, side, 0);

            for (var i = 1; i <= length; i++)
            {
                var color = i < length ? DoorColorAt(board, side, i) : runColor;
                if (i < length && color == runColor)
                {
                    continue;
                }

                if (runColor == NoDoor)
                {
                    walls.Add(Stretched(board, art, art.Wall, art.WallModelRotation, side, runStart, i, turn));
                }
                else
                {
                    var piece = Stretched(board, art, art.Door, art.DoorModelRotation, side, runStart, i, turn);
                    var middle = EdgePoint(board, side, (runStart + i) * 0.5f);
                    var pivot = new Vector3(middle.x * cellSize, 0f, middle.y * cellSize);
                    doors.Add(new DoorRun(side, runStart, i, runColor, pivot, piece));
                }

                runStart = i;
                runColor = color;
            }
        }

        /// <summary>One piece stretched along a side from cell <paramref name="from"/> up to <paramref name="to"/>.</summary>
        private CombineInstance Stretched(Board board, BoardArt art, Mesh mesh, Quaternion rotation, BoardSide side, int from, int to,
            Quaternion turn)
        {
            var stretch = (to - from) * ArtSpace.CellSize / LengthAlongX(mesh, rotation);
            return WallPiece(art, mesh, rotation, EdgePoint(board, side, (from + to) * 0.5f), turn, stretch);
        }

        /// <summary>The color of the door at a cell along a side, or <see cref="NoDoor"/> for wall.</summary>
        private static int DoorColorAt(Board board, BoardSide side, int alongEdge)
        {
            var door = DoorAt(board, side, alongEdge);
            return door != null ? door.Color : NoDoor;
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
            AddRenderer(staticParts, "Board", mesh, art.GroundMaterial, art.WallMaterial);
        }

        private static Mesh Combine(string meshName, List<CombineInstance> pieces)
        {
            var mesh = new Mesh { name = meshName, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(pieces.ToArray(), mergeSubMeshes: true, useMatrices: true);
            return mesh;
        }

        private MeshRenderer AddRenderer(Transform parent, string partName, Mesh mesh, params Material[] materials)
        {
            builtMeshes.Add(mesh);
            var part = new GameObject(partName) { isStatic = parent.gameObject.isStatic };
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var meshRenderer = part.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            return meshRenderer;
        }
    }
}
