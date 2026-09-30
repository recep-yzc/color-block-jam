using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    internal static class BlockMeshBuilder
    {
        private static readonly GridPoint[] CellsAroundCorner = { new(-1, -1), new(0, -1), new(-1, 0), new(0, 0) };

        public static Mesh Build(BoardBlock block, BoardArt art, Color color)
        {
            var pivot = new Vector3(block.MinX + block.MaxX + 1, 0f, block.MinY + block.MaxY + 1) * (ArtSpace.CellSize * 0.5f);
            var cells = new HashSet<GridPoint>();
            var corners = new HashSet<GridPoint>();
            foreach (var cell in block.Cells)
            {
                cells.Add(cell);
                corners.Add(cell);
                corners.Add(cell + new GridPoint(1, 0));
                corners.Add(cell + new GridPoint(0, 1));
                corners.Add(cell + new GridPoint(1, 1));
            }

            var pieces = new List<CombineInstance>();
            foreach (var corner in corners)
            {
                AddCornerPieces(corner, cells, art, pivot, pieces);
            }

            var blockVertices = 0;
            foreach (var piece in pieces)
            {
                blockVertices += piece.mesh.vertexCount;
            }

            if (block.Axis != BlockAxis.Free)
            {
                pieces.Add(Arrow(block, art, pivot));
            }

            var mesh = new Mesh { name = $"Block {block.Id}" };
            mesh.CombineMeshes(pieces.ToArray(), mergeSubMeshes: true, useMatrices: true);
            mesh.RecalculateBounds();
            MeshTint.Paint(mesh, color, blockVertices, art.ArrowColorOn(color));
            AddOutlineNormals(mesh);
            return mesh;
        }

        private static CombineInstance Arrow(BoardBlock block, BoardArt art, Vector3 pivot)
        {
            var run = BlockMarks.FindArrow(block.Cells, block.Axis);
            var position = new Vector3(run.CenterX, 0f, run.CenterY) * ArtSpace.CellSize - pivot + Vector3.up * art.ArrowHeight;

            var turn = block.Axis == BlockAxis.Horizontal ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            return new CombineInstance
            {
                mesh = art.ArrowFor(run.Length),
                transform = Matrix4x4.TRS(position, turn * ArtSpace.LayFlat, Vector3.one)
            };
        }

        private static void AddOutlineNormals(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var sums = new Dictionary<Vector3Int, Vector3>(vertices.Length);
            for (var i = 0; i < vertices.Length; i++)
            {
                var place = Weld(vertices[i]);
                sums.TryGetValue(place, out var sum);
                sums[place] = sum + normals[i];
            }

            var smoothed = new Vector3[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                smoothed[i] = sums[Weld(vertices[i])].normalized;
            }

            mesh.SetUVs(3, smoothed);
        }

        private static Vector3Int Weld(Vector3 position) => Vector3Int.RoundToInt(position * 1000f);

        private static void AddCornerPieces(GridPoint corner, HashSet<GridPoint> cells, BoardArt art, Vector3 pivot,
            List<CombineInstance> pieces)
        {
            var filled = 0;
            var empty = new GridPoint();
            foreach (var offset in CellsAroundCorner)
            {
                if (cells.Contains(corner + offset))
                {
                    filled++;
                }
                else
                {
                    empty = offset;
                }
            }

            var cornerPosition = new Vector3(corner.X * ArtSpace.CellSize, 0f, corner.Y * ArtSpace.CellSize) - pivot;

            if (filled == 3)
            {
                var turn = ArtSpace.TurnToDiagonal(empty.X == 0 ? 1 : -1, empty.Y == 0 ? 1 : -1);
                pieces.Add(Piece(art.BlockInnerCorner, cornerPosition, turn, Vector3.zero, art.BlockBaseDepth));
                return;
            }

            foreach (var offset in CellsAroundCorner)
            {
                var cell = corner + offset;
                if (!cells.Contains(cell))
                {
                    continue;
                }

                var signX = offset.X == -1 ? 1 : -1;
                var signY = offset.Y == -1 ? 1 : -1;
                var hasSide = cells.Contains(cell + new GridPoint(signX, 0));
                var hasFront = cells.Contains(cell + new GridPoint(0, signY));
                var quarterCenter = cornerPosition + new Vector3(-signX, 0f, -signY) * (ArtSpace.QuarterSize * 0.5f);
                var toModelOrigin = new Vector3(-0.5f, 0f, -0.5f) * ArtSpace.QuarterSize;

                if (!hasSide && !hasFront)
                {
                    pieces.Add(Piece(art.BlockOuterCorner, quarterCenter, ArtSpace.TurnToDiagonal(signX, signY), toModelOrigin, art.BlockBaseDepth));
                }
                else if (hasSide && !hasFront)
                {
                    pieces.Add(Piece(art.BlockEdge, quarterCenter, ArtSpace.TurnToSide(0, signY), toModelOrigin, art.BlockBaseDepth));
                }
                else if (!hasSide)
                {
                    pieces.Add(Piece(art.BlockEdge, quarterCenter, ArtSpace.TurnToSide(signX, 0), toModelOrigin, art.BlockBaseDepth));
                }
                else
                {
                    pieces.Add(Piece(art.BlockCenter, quarterCenter, Quaternion.identity, toModelOrigin, art.BlockBaseDepth));
                }
            }
        }

        private static CombineInstance Piece(Mesh mesh, Vector3 pivot, Quaternion turn, Vector3 toModelOrigin, float baseDepth)
        {
            var place = Matrix4x4.TRS(pivot, turn, Vector3.one);
            var model = Matrix4x4.TRS(toModelOrigin + Vector3.up * baseDepth, ArtSpace.LayFlat, Vector3.one);
            return new CombineInstance { mesh = mesh, transform = place * model };
        }
    }
}
