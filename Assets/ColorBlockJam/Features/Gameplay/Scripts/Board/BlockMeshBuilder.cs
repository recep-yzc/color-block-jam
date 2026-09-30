using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Builds one mesh for a block from the modular quarter pieces, in art units (a cell is 2 units, a quarter 1).
    /// Every grid corner of the block is looked at with the four cells around it:
    /// a lone quarter is an outer corner, a quarter with one neighbor is an edge, a full corner is center pieces,
    /// and a corner with three cells gets one inner corner piece with a concave bevel.
    /// An arrow block also gets its arrow, laid along a straight run of its cells on its axis.
    /// The mesh origin is the middle of the block's bounds, so it scales and turns around its center.
    /// The colors are in the vertices, and UV channel 3 holds the smoothed normals the outline and the ice push along.
    /// </summary>
    internal static class BlockMeshBuilder
    {
        // The four cells around a grid corner, as offsets from the corner.
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

            // The combined mesh keeps the pieces' order, so the block's vertices come first and the arrow's after.
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

        /// <summary>The arrow of an arrow block, where <see cref="BlockMarks"/> puts it, pointing along the block's axis.</summary>
        private static CombineInstance Arrow(BoardBlock block, BoardArt art, Vector3 pivot)
        {
            var run = BlockMarks.FindArrow(block.Cells, block.Axis);
            var position = new Vector3(run.CenterX, 0f, run.CenterY) * ArtSpace.CellSize - pivot + Vector3.up * art.ArrowHeight;

            // The arrow model points along its Y, which lying flat is the board's Y; a horizontal one turns a quarter.
            var turn = block.Axis == BlockAxis.Horizontal ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            return new CombineInstance
            {
                mesh = art.ArrowFor(run.Length),
                transform = Matrix4x4.TRS(position, turn * ArtSpace.LayFlat, Vector3.one)
            };
        }

        /// <summary>
        /// Writes into UV channel 3 each vertex's normal averaged with every vertex at the same place. The outline
        /// shader pushes its hull along these, so edges where the pieces' normals differ do not split the rim.
        /// </summary>
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

        // Vertices closer than a thousandth of a unit are the same place.
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
                // The open quarter of the model is +X +Y; turn it toward the empty cell.
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

                // Direction from this cell's quarter toward the corner.
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
