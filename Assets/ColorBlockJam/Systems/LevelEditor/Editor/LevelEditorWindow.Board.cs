using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void DrawBoard()
        {
            for (var x = 0; x < level.Width; x++)
            {
                for (var y = 0; y < level.Height; y++)
                {
                    if (level.IsHole(new GridPoint(x, y)))
                    {
                        DrawHole(new GridPoint(x, y));
                    }
                    else
                    {
                        EditorGUI.DrawRect(Shrink(CellRect(x, y), 1f), CellColor);
                    }
                }
            }

            foreach (BoardSide side in System.Enum.GetValues(typeof(BoardSide)))
            {
                for (var slot = 0; slot < level.SlotCount(side); slot++)
                {
                    var bar = SlotBar(side, slot);
                    var doorColor = level.GetDoor(side, slot);
                    EditorGUI.DrawRect(bar, doorColor == EditableLevel.NoDoor ? WallColor : palette.GetColor(doorColor));
                    if (doorColor != EditableLevel.NoDoor)
                    {
                        DrawArrow(bar, side);
                    }
                }
            }

            if (preview != null)
            {
                foreach (var block in preview.Blocks)
                {
                    if (!block.IsCleared)
                    {
                        var cells = new List<GridPoint>();
                        foreach (var cell in block.Cells)
                        {
                            cells.Add(block.Position + cell);
                        }

                        DrawBlock(cells, palette.GetColor(block.Color), 1f, outline: null, block.Axis, preview.IceLeft(block));
                    }
                }

                return;
            }

            for (var i = 0; i < level.Blocks.Count; i++)
            {
                if (i == movingBlock)
                {
                    continue;
                }

                var block = level.Blocks[i];
                var outline = i == selectedBlock ? SelectedOutline : problemBlocks.Contains(i) ? ProblemOutline : (Color?)null;
                DrawBlock(block.Cells, palette.GetColor(block.Color), 1f, outline, block.Axis, block.Ice);
            }

            DrawGhost();

            if (hasHover && tool != Tool.Stamp)
            {
                var rect = hover.IsCell ? CellRect(hover.Cell.X, hover.Cell.Y) : SlotBar(hover.Side, hover.Slot);
                EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.15f));
            }
        }

        private void DrawGhost()
        {
            if (movingBlock >= 0)
            {
                var block = level.Blocks[movingBlock];
                var moved = Offset(block.Cells, moveOffset);
                var fits = level.Fits(moved, movingBlock);
                DrawBlock(moved, fits ? palette.GetColor(block.Color) : Color.red, fits ? 0.85f : 0.6f, SelectedOutline, block.Axis, block.Ice);
                return;
            }

            if (tool == Tool.Stamp && hasHover && hover.IsCell)
            {
                var cells = Offset(shapes[shapeIndex], hover.Cell);
                var fits = level.Fits(cells);
                DrawBlock(cells, fits ? palette.GetColor(color) : Color.red, 0.5f, outline: null);
            }
        }

        private void DrawBlock(IReadOnlyList<GridPoint> cells, Color fill, float alpha, Color? outline,
            BlockAxis axis = BlockAxis.Free, int ice = 0)
        {
            fill.a = alpha;
            var edge = new Color(fill.r * 0.55f, fill.g * 0.55f, fill.b * 0.55f, alpha);
            const float border = 3f;
            foreach (var cell in cells)
            {
                var rect = CellRect(cell.X, cell.Y);
                var left = !Contains(cells, cell + new GridPoint(-1, 0));
                var right = !Contains(cells, cell + new GridPoint(1, 0));
                var down = !Contains(cells, cell + new GridPoint(0, -1));
                var up = !Contains(cells, cell + new GridPoint(0, 1));

                var inner = new Rect(rect.x + (left ? 2f : 0f), rect.y + (up ? 2f : 0f),
                    rect.width - (left ? 2f : 0f) - (right ? 2f : 0f), rect.height - (up ? 2f : 0f) - (down ? 2f : 0f));
                EditorGUI.DrawRect(inner, edge);
                var face = new Rect(inner.x + (left ? border : 0f), inner.y + (up ? border : 0f),
                    inner.width - (left ? border : 0f) - (right ? border : 0f), inner.height - (up ? border : 0f) - (down ? border : 0f));
                EditorGUI.DrawRect(face, fill);
                if (ice > 0)
                {
                    EditorGUI.DrawRect(face, new Color(IceTint.r, IceTint.g, IceTint.b, IceTint.a * alpha));
                }

                if (outline is { } line)
                {
                    if (left) EditorGUI.DrawRect(new Rect(inner.x, inner.y, 2f, inner.height), line);
                    if (right) EditorGUI.DrawRect(new Rect(inner.xMax - 2f, inner.y, 2f, inner.height), line);
                    if (up) EditorGUI.DrawRect(new Rect(inner.x, inner.y, inner.width, 2f), line);
                    if (down) EditorGUI.DrawRect(new Rect(inner.x, inner.yMax - 2f, inner.width, 2f), line);
                }
            }

            var span = ToArray(cells);
            if (axis != BlockAxis.Free)
            {
                DrawAxisArrow(BlockMarks.FindArrow(span, axis), axis, alpha);
            }

            if (ice > 0)
            {
                var cell = BlockMarks.FindIceCell(span);
                iceCountStyle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                iceCountStyle.fontSize = Mathf.RoundToInt(cellSize * 0.45f);
                iceCountStyle.normal.textColor = IceCountColor;
                GUI.Label(CellRect(cell.X, cell.Y), ice.ToString(), iceCountStyle);
            }
        }

        private void DrawAxisArrow(ArrowRun run, BlockAxis axis, float alpha)
        {
            var center = CellPoint(run.CenterX, run.CenterY);
            var direction = axis == BlockAxis.Horizontal ? Vector2.right : Vector2.up;
            var half = (run.Length * 0.5f - 0.15f) * cellSize;
            var head = cellSize * 0.22f;
            var thickness = cellSize * 0.1f;
            var color = new Color(ArrowColor.r, ArrowColor.g, ArrowColor.b, ArrowColor.a * alpha);

            var shaftLength = (half - head) * 2f;
            var shaft = axis == BlockAxis.Horizontal
                ? new Rect(center.x - shaftLength * 0.5f, center.y - thickness * 0.5f, shaftLength, thickness)
                : new Rect(center.x - thickness * 0.5f, center.y - shaftLength * 0.5f, thickness, shaftLength);
            EditorGUI.DrawRect(shaft, color);

            Handles.color = color;
            DrawArrowHead(center + direction * half, direction, head);
            DrawArrowHead(center - direction * half, -direction, head);
        }

        private static void DrawArrowHead(Vector2 tip, Vector2 direction, float size)
        {
            var side = new Vector2(-direction.y, direction.x) * (size * 0.8f);
            var back = tip - direction * size;
            Handles.DrawAAConvexPolygon(tip, back + side, back - side);
        }

        private Vector2 CellPoint(float x, float y)
        {
            return new Vector2(boardRect.x + (x + 1f) * cellSize, boardRect.y + (level.Height + 1f - y) * cellSize);
        }

        private static GridPoint[] ToArray(IReadOnlyList<GridPoint> cells)
        {
            var array = new GridPoint[cells.Count];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = cells[i];
            }

            return array;
        }

        private static void DrawArrow(Rect bar, BoardSide side)
        {
            var center = bar.center;
            var size = Mathf.Min(bar.width, bar.height) * 0.35f;
            var color = new Color(1f, 1f, 1f, 0.8f);
            var isVertical = side is BoardSide.Bottom or BoardSide.Top;
            var step = side is BoardSide.Bottom or BoardSide.Left ? 1f : -1f;
            for (var i = 0; i < 3; i++)
            {
                var length = size * (1f - i * 0.3f);
                var offset = (i - 1) * size * 0.3f * -step;
                var rect = isVertical
                    ? new Rect(center.x - length, center.y + offset - 1f, length * 2f, 2f)
                    : new Rect(center.x + offset - 1f, center.y - length, 2f, length * 2f);
                EditorGUI.DrawRect(rect, color);
            }
        }

        private void DrawHole(GridPoint cell)
        {
            var rect = CellRect(cell.X, cell.Y);
            var rim = cellSize * 0.25f;
            EditorGUI.DrawRect(rect, Background);

            if (level.IsFloor(new GridPoint(cell.X - 1, cell.Y)))
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rim, rect.height), WallColor);
            }

            if (level.IsFloor(new GridPoint(cell.X + 1, cell.Y)))
            {
                EditorGUI.DrawRect(new Rect(rect.xMax - rim, rect.y, rim, rect.height), WallColor);
            }

            if (level.IsFloor(new GridPoint(cell.X, cell.Y + 1)))
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rim), WallColor);
            }

            if (level.IsFloor(new GridPoint(cell.X, cell.Y - 1)))
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - rim, rect.width, rim), WallColor);
            }

            for (var dx = -1; dx <= 1; dx += 2)
            {
                for (var dy = -1; dy <= 1; dy += 2)
                {
                    if (!level.IsFloor(new GridPoint(cell.X + dx, cell.Y)) && !level.IsFloor(new GridPoint(cell.X, cell.Y + dy)) &&
                        level.IsFloor(new GridPoint(cell.X + dx, cell.Y + dy)))
                    {
                        var x = dx < 0 ? rect.x : rect.xMax - rim;
                        var y = dy > 0 ? rect.y : rect.yMax - rim;
                        EditorGUI.DrawRect(new Rect(x, y, rim, rim), WallColor);
                    }
                }
            }
        }

        private Rect CellRect(int x, int y)
        {
            return new Rect(boardRect.x + (x + 1) * cellSize, boardRect.y + (level.Height - y) * cellSize, cellSize, cellSize);
        }

        private Rect SlotBar(BoardSide side, int slot)
        {
            var thickness = cellSize * 0.42f;
            switch (side)
            {
                case BoardSide.Bottom:
                {
                    var cell = CellRect(slot, -1);
                    return new Rect(cell.x + 1f, cell.y + 1f, cell.width - 2f, thickness);
                }
                case BoardSide.Top:
                {
                    var cell = CellRect(slot, level.Height);
                    return new Rect(cell.x + 1f, cell.yMax - thickness - 1f, cell.width - 2f, thickness);
                }
                case BoardSide.Left:
                {
                    var cell = CellRect(-1, slot);
                    return new Rect(cell.xMax - thickness - 1f, cell.y + 1f, thickness, cell.height - 2f);
                }
                default:
                {
                    var cell = CellRect(level.Width, slot);
                    return new Rect(cell.x + 1f, cell.y + 1f, thickness, cell.height - 2f);
                }
            }
        }

        private static Rect Shrink(Rect rect, float amount) => new(rect.x + amount, rect.y + amount, rect.width - amount * 2f, rect.height - amount * 2f);

        private static Rect Grow(Rect rect, float amount) => Shrink(rect, -amount);
    }
}
