using System.Collections.Generic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void Change(System.Action edit)
        {
            var control = GUIUtility.hotControl;
            if (control == 0 || control != undoControl)
            {
                RecordUndo();
            }

            undoControl = control;
            edit();
            OnLevelChanged();
        }

        private void Select(int block)
        {
            if (selectedBlock != block)
            {
                selectedBlock = block;
                isLayoutStale = true;
            }
        }

        private void OnLevelChanged()
        {
            IsDirty = true;
            isLayoutStale = true;
            revision++;
            StopPreview();
            CancelValidation();
            var data = level.ToData();
            levelJson = LevelSerializer.ToJson(data);
            problems = LevelDiagnostics.Find(data);
            usedColors.Clear();
            foreach (var block in level.Blocks)
            {
                usedColors.Add(block.Color);
            }

            colorCount = usedColors.Count;
            problemBlocks.Clear();
            foreach (var problem in problems)
            {
                if (problem.Block >= 0)
                {
                    problemBlocks.Add(problem.Block);
                }
            }
            if (selectedBlock >= level.Blocks.Count)
            {
                selectedBlock = -1;
            }

            Repaint();
        }

        private void SetLevel(EditableLevel newLevel, bool dirty)
        {
            level = newLevel;
            selectedBlock = -1;
            result = null;
            OnLevelChanged();
            IsDirty = dirty;
        }

        private void RecordUndo()
        {
            undoHistory.Add(levelJson);
            if (undoHistory.Count > HistoryLimit)
            {
                undoHistory.RemoveAt(0);
            }

            redoHistory.Clear();
        }

        private void Undo()
        {
            if (undoHistory.Count == 0)
            {
                return;
            }

            redoHistory.Add(levelJson);
            level = EditableLevel.From(LevelSerializer.FromJson(Pop(undoHistory)));
            OnLevelChanged();
        }

        private void Redo()
        {
            if (redoHistory.Count == 0)
            {
                return;
            }

            undoHistory.Add(levelJson);
            level = EditableLevel.From(LevelSerializer.FromJson(Pop(redoHistory)));
            OnLevelChanged();
        }

        private void ResetHistory()
        {
            undoHistory.Clear();
            redoHistory.Clear();
        }

        private static string Pop(List<string> history)
        {
            var last = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);
            return last;
        }

        private bool ConfirmDiscard()
        {
            return !IsDirty || EditorUtility.DisplayDialog("Level Editor", "The level has unsaved changes. Discard them?", "Discard", "Keep Editing");
        }
    }
}
