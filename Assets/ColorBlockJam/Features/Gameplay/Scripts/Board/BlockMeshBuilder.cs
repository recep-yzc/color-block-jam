using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Builds one mesh for a block from the modular quarter pieces, in art units (a cell is 2 units, a quarter 1).
    /// Every grid corner of the block is looked at with the four cells around it:
    /// a lone quarter is an outer corner, a quarter with one neighbor is an edge, a full corner is center pieces,
    /// and a corner with three cells gets one inner corner piece with a concave bevel.
    /// The mesh origin is the middle of the block's bounds, so it scales and turns around its center.
    /// </summary>
    internal static class BlockMeshBuilder
    {
        // The four cells around a grid corner, as offsets from the corner.
        private static readonly GridPoint[] CellsAroundCorner = { new(-1, -1), new(0, -1), new(-1, 0), new(0, 0) };

        public static Mesh Build(BoardBlock block, BoardArt art)
        {
            var pivot = new Vector3(block.MinX + block.MaxX + 1, 0f, block.MinY + block.MaxY + 1) * (ArtSpace.CellSize * 0.5f);
            var cells = new HashSet<GridPoint>(block.Cells);
            var corners = new HashSet<GridPoint>();
            foreach (var cell in block.Cells)
            {
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

            var mesh = new Mesh { name = $"Block {block.Id}" };
            mesh.CombineMeshes(pieces.ToArray(), mergeSubMeshes: true, useMatrices: true);
            mesh.RecalculateBounds();
            return mesh;
        }

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
