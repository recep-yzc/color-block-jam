using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorBlockJam.Gameplay
{
    internal sealed class BoardMeshBuilder
    {
        private const int NoDoor = -1;

        private readonly Board board;
        private readonly BoardArt art;
        private readonly BlockPalette palette;
        private readonly float cellSize;
        private readonly float artScale;
        private readonly float wallBand;

        public BoardMeshBuilder(Board board, BoardArt art, BlockPalette palette, float cellSize)
        {
            this.board = board;
            this.art = art;
            this.palette = palette;
            this.cellSize = cellSize;
            artScale = cellSize / ArtSpace.CellSize;
            wallBand = art.WallHalfThickness;
        }

        public Mesh BuildBoard(List<DoorMesh> doors)
        {
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
            BuildSide(BoardSide.Bottom, board.Width, walls, doors);
            BuildSide(BoardSide.Top, board.Width, walls, doors);
            BuildSide(BoardSide.Left, board.Height, walls, doors);
            BuildSide(BoardSide.Right, board.Height, walls, doors);
            BuildCorners(walls);
            BuildHoleRims(walls);

            var groundMesh = Combine("Ground", ground);
            var wallMesh = Combine("Walls", walls);
            var mesh = new Mesh { name = "Board", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(new[] { new CombineInstance { mesh = groundMesh }, new CombineInstance { mesh = wallMesh } },
                mergeSubMeshes: false, useMatrices: false);
            Object.Destroy(groundMesh);
            Object.Destroy(wallMesh);
            return mesh;
        }

        public Mesh BuildFloor()
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
            return mesh;
        }

        private void BuildSide(BoardSide side, int length, List<CombineInstance> walls, List<DoorMesh> doors)
        {
            var turn = side is BoardSide.Left or BoardSide.Right ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            var runStart = 0;
            var runColor = DoorColorAt(side, 0);

            for (var i = 1; i <= length; i++)
            {
                var color = i < length ? DoorColorAt(side, i) : runColor;
                if (i < length && color == runColor)
                {
                    continue;
                }

                if (runColor == NoDoor)
                {
                    walls.Add(Stretched(art.Wall, art.WallModelRotation, side, runStart, i, turn));
                }
                else
                {
                    doors.Add(BuildDoor(side, runStart, i, runColor, turn));
                }

                runStart = i;
                runColor = color;
            }
        }

        private DoorMesh BuildDoor(BoardSide side, int from, int to, int color, Quaternion turn)
        {
            var middle = EdgePoint(side, (from + to) * 0.5f);
            var pivot = new Vector3(middle.x * cellSize, 0f, middle.y * cellSize);
            var toPivot = Matrix4x4.Translate(-pivot);

            var body = Stretched(art.Door, art.DoorModelRotation, side, from, to, turn);
            body.transform = toPivot * body.transform;
            var arrow = DoorArrow(side, middle);
            arrow.transform = toPivot * arrow.transform;

            var mesh = Combine($"Door {side} {from}", new List<CombineInstance> { body, arrow });
            var doorColor = palette.GetColor(color);
            MeshTint.Paint(mesh, doorColor, art.Door.vertexCount, art.ArrowColorOn(doorColor));
            return new DoorMesh(side, from, to, pivot, mesh);
        }

        private CombineInstance Stretched(Mesh mesh, Quaternion rotation, BoardSide side, int from, int to, Quaternion turn)
        {
            var stretch = (to - from) * ArtSpace.CellSize / LengthAlongX(mesh, rotation);
            return WallPiece(mesh, rotation, EdgePoint(side, (from + to) * 0.5f), turn, stretch);
        }

        private CombineInstance DoorArrow(BoardSide side, Vector2 middle)
        {
            var arrow = art.DoorArrow;
            var model = Matrix4x4.Scale(Vector3.one * art.DoorArrowScale) * Matrix4x4.Rotate(art.DoorArrowModelRotation) *
                        Matrix4x4.Translate(-arrow.bounds.center);
            var piece = Piece(arrow, middle, DoorTop() + art.DoorArrowLift, Quaternion.Euler(0f, ExitAngle(side), 0f));
            piece.transform *= model;
            return piece;
        }

        private float DoorTop()
        {
            var bounds = art.Door.bounds;
            var turned = Matrix4x4.Rotate(art.DoorModelRotation);
            var extents = bounds.extents;
            var halfHeight = Mathf.Abs(turned.m10) * extents.x + Mathf.Abs(turned.m11) * extents.y + Mathf.Abs(turned.m12) * extents.z;
            return art.WallHeightOffset + bounds.center.y + halfHeight;
        }

        private static float ExitAngle(BoardSide side)
        {
            return side switch
            {
                BoardSide.Top => 0f,
                BoardSide.Right => 90f,
                BoardSide.Bottom => 180f,
                _ => 270f
            };
        }

        private int DoorColorAt(BoardSide side, int alongEdge)
        {
            var door = board.DoorAt(side, alongEdge);
            return door != null ? door.Color : NoDoor;
        }

        private void BuildCorners(List<CombineInstance> walls)
        {
            var outside = -wallBand;
            var right = board.Width + wallBand;
            var top = board.Height + wallBand;

            walls.Add(WallPiece(art.WallCorner, art.CornerModelRotation, new Vector2(outside, outside), Quaternion.identity, 1f));
            walls.Add(WallPiece(art.WallCorner, art.CornerModelRotation, new Vector2(outside, top), Quaternion.Euler(0f, 90f, 0f), 1f));
            walls.Add(WallPiece(art.WallCorner, art.CornerModelRotation, new Vector2(right, top), Quaternion.Euler(0f, 180f, 0f), 1f));
            walls.Add(WallPiece(art.WallCorner, art.CornerModelRotation, new Vector2(right, outside), Quaternion.Euler(0f, 270f, 0f), 1f));
        }

        private void BuildHoleRims(List<CombineInstance> walls)
        {
            for (var y = 0; y < board.Height; y++)
            {
                AddRimRuns(walls, y, isAlongX: true, facing: -1);
                AddRimRuns(walls, y, isAlongX: true, facing: 1);
            }

            for (var x = 0; x < board.Width; x++)
            {
                AddRimRuns(walls, x, isAlongX: false, facing: -1);
                AddRimRuns(walls, x, isAlongX: false, facing: 1);
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
                        AddRimCorner(walls, hole, dx, dy);
                    }
                }
            }
        }

        private void AddRimRuns(List<CombineInstance> walls, int line, bool isAlongX, int facing)
        {
            var length = isAlongX ? board.Width : board.Height;
            var turn = isAlongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
            var across = facing < 0 ? line + wallBand : line + 1f - wallBand;

            for (var along = 0; along < length; along++)
            {
                if (!FacesFloor(line, along, isAlongX, facing))
                {
                    continue;
                }

                var first = along;
                while (along + 1 < length && FacesFloor(line, along + 1, isAlongX, facing))
                {
                    along++;
                }

                var from = first + (IsFloorAt(line, first - 1, isAlongX) ? 2f * wallBand : 0f);
                var to = along + 1 - (IsFloorAt(line, along + 1, isAlongX) ? 2f * wallBand : 0f);
                var middle = (from + to) * 0.5f;
                var center = isAlongX ? new Vector2(middle, across) : new Vector2(across, middle);
                var stretch = (to - from) * ArtSpace.CellSize / LengthAlongX(art.Wall, art.WallModelRotation);
                walls.Add(WallPiece(art.Wall, art.WallModelRotation, center, turn, stretch));
            }
        }

        private bool FacesFloor(int line, int along, bool isAlongX, int facing)
        {
            return isAlongX
                ? board.IsHole(along, line) && board.IsFloor(along, line + facing)
                : board.IsHole(line, along) && board.IsFloor(line + facing, along);
        }

        private bool IsFloorAt(int line, int along, bool isAlongX)
        {
            return isAlongX ? board.IsFloor(along, line) : board.IsFloor(line, along);
        }

        private void AddRimCorner(List<CombineInstance> walls, GridPoint hole, int dx, int dy)
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
            var center = new Vector2(hole.X + (dx < 0 ? wallBand : 1f - wallBand), hole.Y + (dy < 0 ? wallBand : 1f - wallBand));
            walls.Add(WallPiece(art.WallCorner, art.CornerModelRotation, center, Quaternion.Euler(0f, angle, 0f), 1f));
        }

        private Vector2 EdgePoint(BoardSide side, float along)
        {
            return side switch
            {
                BoardSide.Bottom => new Vector2(along, -wallBand),
                BoardSide.Top => new Vector2(along, board.Height + wallBand),
                BoardSide.Left => new Vector2(-wallBand, along),
                _ => new Vector2(board.Width + wallBand, along)
            };
        }

        private CombineInstance Piece(Mesh mesh, Vector2 cell, float height, Quaternion turn)
        {
            var position = new Vector3(cell.x * cellSize, height * artScale, cell.y * cellSize);
            return new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, turn, Vector3.one * artScale) };
        }

        private CombineInstance WallPiece(Mesh mesh, Quaternion modelRotation, Vector2 cell, Quaternion turn, float stretch)
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

        private void AddFloorRow(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, int from, int to, int row, float height)
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
            var mesh = new Mesh { name = meshName, indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(pieces.ToArray(), mergeSubMeshes: true, useMatrices: true);
            return mesh;
        }
    }
}
