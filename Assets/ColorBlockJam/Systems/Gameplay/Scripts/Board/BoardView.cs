using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class BoardView : MonoBehaviour
    {
        private const int NoDoor = -1;
        private const float WallBand = 0.25f;

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

            staticParts = new GameObject("Board Parts") { isStatic = true }.transform;
            staticParts.SetParent(transform, false);
            doorParts = new GameObject("Doors").transform;
            doorParts.SetParent(transform, false);

            var ground = new List<CombineInstance>();
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    if (board.IsFloor(x, y))
                    {
                        ground.Add(Piece(art.GroundTile, new Vector2(x + 0.5f, y + 0.5f), 0f, Quaternion.identity));
                    }
                }
            }

            var walls = new List<CombineInstance>();
            var doors = new List<DoorRun>();
            BuildSide(board, BoardSide.Bottom, board.Width, art, walls, doors);
            BuildSide(board, BoardSide.Top, board.Width, art, walls, doors);
            BuildSide(board, BoardSide.Left, board.Height, art, walls, doors);
            BuildSide(board, BoardSide.Right, board.Height, art, walls, doors);
            BuildCorners(board, art, walls);
            BuildHoleRims(board, art, walls);
            AddBoard(ground, walls, art);
            AddFloor(board, art);

            foreach (var door in doors)
            {
                var piece = door.Piece;
                piece.transform = Matrix4x4.Translate(-door.Pivot) * piece.transform;
                var doorName = $"Door {door.Side} {door.From}";
                var doorMesh = Combine(doorName, new List<CombineInstance> { piece });
                MeshTint.Paint(doorMesh, palette.GetColor(door.Color));
                var doorRenderer = AddRenderer(doorParts, doorName, doorMesh, art.DoorMaterial);
                doorRenderer.transform.localPosition = door.Pivot;

                var doorView = doorRenderer.gameObject.AddComponent<DoorView>();
                doorView.Initialize(door.Side, door.From, door.To, config);
                doorViews.Add(doorView);
            }

            var size = new Vector3((board.Width + 1) * cellSize, cellSize, (board.Height + 1) * cellSize);
            WorldBounds = new Bounds(CellToWorld(new Vector2(board.Width * 0.5f, board.Height * 0.5f)), size);
        }

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

        public Vector3 CellToWorld(Vector2 cell)
        {
            return transform.position + new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
        }

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

        public void Clear()
        {
            foreach (var mesh in builtMeshes)
            {
                Destroy(mesh);
            }

            builtMeshes.Clear();
            doorViews.Clear();
            if (staticParts != null)
            {
                Destroy(staticParts.gameObject);
            }

            if (doorParts != null)
            {
                Destroy(doorParts.gameObject);
            }
        }

        private void OnDestroy()
        {
            foreach (var mesh in builtMeshes)
            {
                Destroy(mesh);
            }
        }

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

        private CombineInstance Stretched(Board board, BoardArt art, Mesh mesh, Quaternion rotation, BoardSide side, int from, int to,
            Quaternion turn)
        {
            var stretch = (to - from) * ArtSpace.CellSize / LengthAlongX(mesh, rotation);
            return WallPiece(art, mesh, rotation, EdgePoint(board, side, (from + to) * 0.5f), turn, stretch);
        }

        private static int DoorColorAt(Board board, BoardSide side, int alongEdge)
        {
            var door = DoorAt(board, side, alongEdge);
            return door != null ? door.Color : NoDoor;
        }

        private void BuildCorners(Board board, BoardArt art, List<CombineInstance> walls)
        {
            const float outside = -WallBand;
            var right = board.Width + WallBand;
            var top = board.Height + WallBand;

            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(outside, outside), Quaternion.identity, 1f));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(outside, top), Quaternion.Euler(0f, 90f, 0f), 1f));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(right, top), Quaternion.Euler(0f, 180f, 0f), 1f));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, new Vector2(right, outside), Quaternion.Euler(0f, 270f, 0f), 1f));
        }

        private void BuildHoleRims(Board board, BoardArt art, List<CombineInstance> walls)
        {
            for (var y = 0; y < board.Height; y++)
            {
                AddRimRuns(board, art, walls, y, isAlongX: true, facing: -1);
                AddRimRuns(board, art, walls, y, isAlongX: true, facing: 1);
            }

            for (var x = 0; x < board.Width; x++)
            {
                AddRimRuns(board, art, walls, x, isAlongX: false, facing: -1);
                AddRimRuns(board, art, walls, x, isAlongX: false, facing: 1);
            }

            foreach (var hole in board.Holes)
            {
                if (!board.IsHole(hole.X, hole.Y))
                {
                    continue;
                }

                for (var dx = -1; dx <= 1; dx += 2)
                {
                    for (var dy = -1; dy <= 1; dy += 2)
                    {
                        AddRimCorner(board, art, walls, hole, dx, dy);
                    }
                }
            }
        }

        private void AddRimRuns(Board board, BoardArt art, List<CombineInstance> walls, int line, bool isAlongX, int facing)
        {
            var length = isAlongX ? board.Width : board.Height;
            var turn = isAlongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
            var across = facing < 0 ? line + WallBand : line + 1f - WallBand;

            for (var along = 0; along < length; along++)
            {
                if (!FacesFloor(board, line, along, isAlongX, facing))
                {
                    continue;
                }

                var first = along;
                while (along + 1 < length && FacesFloor(board, line, along + 1, isAlongX, facing))
                {
                    along++;
                }

                var from = first + (IsFloorAt(board, line, first - 1, isAlongX) ? 2f * WallBand : 0f);
                var to = along + 1 - (IsFloorAt(board, line, along + 1, isAlongX) ? 2f * WallBand : 0f);
                var middle = (from + to) * 0.5f;
                var center = isAlongX ? new Vector2(middle, across) : new Vector2(across, middle);
                var stretch = (to - from) * ArtSpace.CellSize / LengthAlongX(art.Wall, art.WallModelRotation);
                walls.Add(WallPiece(art, art.Wall, art.WallModelRotation, center, turn, stretch));
            }
        }

        private static bool FacesFloor(Board board, int line, int along, bool isAlongX, int facing)
        {
            return isAlongX
                ? board.IsHole(along, line) && board.IsFloor(along, line + facing)
                : board.IsHole(line, along) && board.IsFloor(line + facing, along);
        }

        private static bool IsFloorAt(Board board, int line, int along, bool isAlongX)
        {
            return isAlongX ? board.IsFloor(along, line) : board.IsFloor(line, along);
        }

        private void AddRimCorner(Board board, BoardArt art, List<CombineInstance> walls, GridPoint hole, int dx, int dy)
        {
            var besideX = board.IsFloor(hole.X + dx, hole.Y);
            var besideY = board.IsFloor(hole.X, hole.Y + dy);
            var isConvex = besideX && besideY;
            var isNotch = !besideX && !besideY && board.IsFloor(hole.X + dx, hole.Y + dy);
            if (!isConvex && !isNotch)
            {
                return;
            }

            var round = isConvex ? new Vector2Int(dx, dy) : new Vector2Int(-dx, -dy);
            var angle = round.x < 0 ? (round.y < 0 ? 0f : 90f) : (round.y > 0 ? 180f : 270f);
            var center = new Vector2(hole.X + (dx < 0 ? WallBand : 1f - WallBand), hole.Y + (dy < 0 ? WallBand : 1f - WallBand));
            walls.Add(WallPiece(art, art.WallCorner, art.CornerModelRotation, center, Quaternion.Euler(0f, angle, 0f), 1f));
        }

        private static Vector2 EdgePoint(Board board, BoardSide side, float along)
        {
            return side switch
            {
                BoardSide.Bottom => new Vector2(along, -WallBand),
                BoardSide.Top => new Vector2(along, board.Height + WallBand),
                BoardSide.Left => new Vector2(-WallBand, along),
                _ => new Vector2(board.Width + WallBand, along)
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

        private CombineInstance WallPiece(BoardArt art, Mesh mesh, Quaternion modelRotation, Vector2 cell, Quaternion turn, float stretch)
        {
            var middle = mesh.bounds.center;
            var alongSide = Matrix4x4.TRS(new Vector3(middle.x * (1f - stretch), 0f, 0f), Quaternion.identity, new Vector3(stretch, 1f, 1f));
            var piece = Piece(mesh, cell, art.WallHeightOffset, turn);
            piece.transform *= alongSide * AroundMiddle(mesh, modelRotation);
            return piece;
        }

        private static Matrix4x4 AroundMiddle(Mesh mesh, Quaternion rotation)
        {
            var middle = mesh.bounds.center;
            return Matrix4x4.TRS(middle - rotation * middle, rotation, Vector3.one);
        }

        private static float LengthAlongX(Mesh mesh, Quaternion rotation)
        {
            var extents = mesh.bounds.extents;
            var turned = Matrix4x4.Rotate(rotation);
            return 2f * (Mathf.Abs(turned.m00) * extents.x + Mathf.Abs(turned.m01) * extents.y + Mathf.Abs(turned.m02) * extents.z);
        }

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

        private void AddFloor(Board board, BoardArt art)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var height = art.FloorHeight * artScale;
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    if (!board.IsFloor(x, y))
                    {
                        continue;
                    }

                    var start = x;
                    while (x + 1 < board.Width && board.IsFloor(x + 1, y))
                    {
                        x++;
                    }

                    AddFloorRow(vertices, uvs, triangles, start, x + 1, y, height);
                }
            }

            var mesh = new Mesh { name = "Floor" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            var floorRenderer = AddRenderer(staticParts, "Floor", mesh, art.FloorMaterial);
            floorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void AddFloorRow(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, int from, int to, int row,
            float height)
        {
            var first = vertices.Count;
            AddFloorCorner(vertices, uvs, from, row, height);
            AddFloorCorner(vertices, uvs, from, row + 1, height);
            AddFloorCorner(vertices, uvs, to, row + 1, height);
            AddFloorCorner(vertices, uvs, to, row, height);
            triangles.Add(first);
            triangles.Add(first + 1);
            triangles.Add(first + 2);
            triangles.Add(first);
            triangles.Add(first + 2);
            triangles.Add(first + 3);
        }

        private void AddFloorCorner(List<Vector3> vertices, List<Vector2> uvs, int x, int y, float height)
        {
            vertices.Add(new Vector3(x * cellSize, height, y * cellSize));
            uvs.Add(new Vector2(x, y));
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
