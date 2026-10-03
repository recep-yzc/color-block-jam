using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void HandleBoardInput(Rect area)
        {
            var current = Event.current;
            var id = GUIUtility.GetControlID(FocusType.Passive);
            var hit = HitTest(current.mousePosition, out var hitResult) && area.Contains(current.mousePosition);
            if (current.type == EventType.MouseDown && area.Contains(current.mousePosition))
            {
                GUIUtility.keyboardControl = 0;
            }

            if (current.type is EventType.MouseMove or EventType.MouseDrag)
            {
                var changed = hasHover != hit || (hit && !SameHit(hover, hitResult));
                hasHover = hit;
                hover = hitResult;
                if (changed)
                {
                    Repaint();
                }
            }

            if (preview != null)
            {
                return;
            }

            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown when hit && GUIUtility.hotControl == 0:
                    if (current.button == 1)
                    {
                        Erase(hitResult);
                    }
                    else if (current.button == 0)
                    {
                        GUIUtility.hotControl = id;
                        PointerDown(hitResult);
                    }

                    current.Use();
                    break;

                case EventType.MouseDrag when GUIUtility.hotControl == id:
                    if (hit)
                    {
                        PointerDrag(hitResult);
                    }

                    current.Use();
                    break;

                case EventType.MouseUp when GUIUtility.hotControl == id:
                    PointerUp();
                    GUIUtility.hotControl = 0;
                    current.Use();
                    break;
            }

            ExitIfLayoutStale();
        }

        private void PointerDown(Hit hit)
        {
            switch (tool)
            {
                case Tool.Draw when hit.IsCell:
                    var owner = level.BlockAt(hit.Cell);
                    if (owner >= 0)
                    {
                        Select(owner);
                        break;
                    }

                    if (level.IsHole(hit.Cell))
                    {
                        break;
                    }

                    RecordUndo();
                    undoControl = GUIUtility.hotControl;
                    drawing = new EditableBlock(color);
                    drawing.Cells.Add(hit.Cell);
                    lastDrawCell = hit.Cell;
                    level.Blocks.Add(drawing);
                    Select(level.Blocks.Count - 1);
                    OnLevelChanged();
                    break;

                case Tool.Stamp when hit.IsCell:
                    var cells = Offset(shapes[shapeIndex], hit.Cell);
                    if (level.Fits(cells))
                    {
                        Change(() =>
                        {
                            var block = new EditableBlock(color);
                            block.Cells.AddRange(cells);
                            level.Blocks.Add(block);
                        });
                        Select(level.Blocks.Count - 1);
                    }

                    break;

                case Tool.Door when hit.IsSlot:
                    paintValue = level.GetDoor(hit.Side, hit.Slot) == color ? EditableLevel.NoDoor : color;
                    Change(() => level.SetDoor(hit.Side, hit.Slot, paintValue));
                    break;

                case Tool.Move when hit.IsCell:
                    movingBlock = level.BlockAt(hit.Cell);
                    moveGrab = hit.Cell;
                    moveOffset = new GridPoint(0, 0);
                    Select(movingBlock);
                    break;

                case Tool.Erase:
                    Erase(hit);
                    break;

                case Tool.Hole when hit.IsCell && level.BlockAt(hit.Cell) < 0:
                    paintHole = !level.IsHole(hit.Cell);
                    Change(() => level.SetHole(hit.Cell, paintHole));
                    break;
            }
        }

        private void PointerDrag(Hit hit)
        {
            switch (tool)
            {
                case Tool.Draw when drawing != null && hit.IsCell:
                    DrawTowards(hit.Cell);
                    break;

                case Tool.Door when hit.IsSlot:
                    if (level.GetDoor(hit.Side, hit.Slot) != paintValue)
                    {
                        Change(() => level.SetDoor(hit.Side, hit.Slot, paintValue));
                    }

                    break;

                case Tool.Move when movingBlock >= 0 && hit.IsCell:
                    moveOffset = hit.Cell - moveGrab;
                    break;

                case Tool.Erase:
                    Erase(hit);
                    break;

                case Tool.Hole when hit.IsCell && level.BlockAt(hit.Cell) < 0 && level.IsHole(hit.Cell) != paintHole:
                    Change(() => level.SetHole(hit.Cell, paintHole));
                    break;
            }

            Repaint();
        }

        private void PointerUp()
        {
            drawing = null;
            if (movingBlock >= 0)
            {
                var block = level.Blocks[movingBlock];
                var moved = Offset(block.Cells, moveOffset);
                if (moveOffset != new GridPoint(0, 0) && level.Fits(moved, movingBlock))
                {
                    Change(() =>
                    {
                        block.Cells.Clear();
                        block.Cells.AddRange(moved);
                    });
                }

                movingBlock = -1;
            }

            Repaint();
        }

        private void Erase(Hit hit)
        {
            if (hit.IsCell)
            {
                var owner = level.BlockAt(hit.Cell);
                if (owner >= 0)
                {
                    Select(-1);
                    Change(() => level.Blocks.RemoveAt(owner));
                }
                else if (level.IsHole(hit.Cell))
                {
                    Change(() => level.SetHole(hit.Cell, false));
                }
            }
            else if (hit.IsSlot && level.GetDoor(hit.Side, hit.Slot) != EditableLevel.NoDoor)
            {
                Change(() => level.SetDoor(hit.Side, hit.Slot, EditableLevel.NoDoor));
            }
        }

        private void HandleShortcuts()
        {
            var current = Event.current;
            if (current.type != EventType.KeyDown)
            {
                return;
            }

            if (GUIUtility.hotControl != 0)
            {
                return;
            }

            var command = current.control || current.command;
            if (command && current.keyCode == KeyCode.Z)
            {
                if (current.shift) Redo(); else Undo();
                current.Use();
            }
            else if (command && current.keyCode == KeyCode.Y)
            {
                Redo();
                current.Use();
            }
            else if (current.keyCode is KeyCode.Delete or KeyCode.Backspace && selectedBlock >= 0 && selectedBlock < level.Blocks.Count
                     && GUIUtility.keyboardControl == 0)
            {
                var index = selectedBlock;
                Select(-1);
                Change(() => level.Blocks.RemoveAt(index));
                current.Use();
            }
            else if (!command && !EditorGUIUtility.editingTextField && current.keyCode >= KeyCode.Alpha0 && current.keyCode <= KeyCode.Alpha9)
            {
                var index = ((int)current.keyCode - (int)KeyCode.Alpha0 + 9) % 10;
                if (index < palette.Count)
                {
                    color = index;
                    current.Use();
                    Repaint();
                }
            }
            else if (!command && !EditorGUIUtility.editingTextField && current.keyCode is KeyCode.LeftArrow or KeyCode.RightArrow)
            {
                var step = current.keyCode == KeyCode.RightArrow ? 1 : -1;
                var next = catalogIndex < 0 ? 0 : catalogIndex + step;
                while (next >= 0 && next < catalog.Count && catalog.Levels[next] == null)
                {
                    next += step;
                }

                current.Use();
                if (next >= 0 && next < catalog.Count)
                {
                    Load(catalog, next);
                    Repaint();
                }
            }
        }

        private bool HitTest(Vector2 mouse, out Hit hit)
        {
            hit = default;
            if (cellSize <= 0f)
            {
                return false;
            }

            var x = Mathf.FloorToInt((mouse.x - boardRect.x) / cellSize) - 1;
            var y = level.Height - Mathf.FloorToInt((mouse.y - boardRect.y) / cellSize);
            var insideX = x >= 0 && x < level.Width;
            var insideY = y >= 0 && y < level.Height;

            if (insideX && insideY)
            {
                hit = new Hit(new GridPoint(x, y));
                return true;
            }

            if (insideX && y == -1) hit = new Hit(BoardSide.Bottom, x);
            else if (insideX && y == level.Height) hit = new Hit(BoardSide.Top, x);
            else if (insideY && x == -1) hit = new Hit(BoardSide.Left, y);
            else if (insideY && x == level.Width) hit = new Hit(BoardSide.Right, y);
            else return false;

            return true;
        }

        private static bool SameHit(Hit a, Hit b)
        {
            return a.IsCell == b.IsCell && a.IsSlot == b.IsSlot && a.Cell == b.Cell && a.Side == b.Side && a.Slot == b.Slot;
        }

        private static List<GridPoint> Offset(IReadOnlyList<GridPoint> cells, GridPoint offset)
        {
            var moved = new List<GridPoint>(cells.Count);
            foreach (var cell in cells)
            {
                moved.Add(cell + offset);
            }

            return moved;
        }

        private static bool Contains(IReadOnlyList<GridPoint> cells, GridPoint cell)
        {
            foreach (var candidate in cells)
            {
                if (candidate == cell)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawTowards(GridPoint target)
        {
            var isChanged = false;
            while (lastDrawCell != target)
            {
                var delta = target - lastDrawCell;
                var step = Math.Abs(delta.X) >= Math.Abs(delta.Y)
                    ? new GridPoint(Math.Sign(delta.X), 0)
                    : new GridPoint(0, Math.Sign(delta.Y));
                var cell = lastDrawCell + step;
                if (level.BlockAt(cell) >= 0 || level.IsHole(cell) || !IsNextTo(drawing.Cells, cell))
                {
                    break;
                }

                drawing.Cells.Add(cell);
                lastDrawCell = cell;
                isChanged = true;
            }

            if (isChanged)
            {
                OnLevelChanged();
            }
        }

        private static bool IsNextTo(List<GridPoint> cells, GridPoint cell)
        {
            foreach (var direction in Directions.All)
            {
                if (cells.Contains(cell + direction.ToOffset()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
