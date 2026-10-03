using System.Collections.Generic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using UnityEditor;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void Change(System.Action edit)
        {
            RecordUndo();
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
            isDirty = true;
            isLayoutStale = true;
            revision++;
            StopPreview();
            var data = level.ToData();
            levelJson = LevelSerializer.ToJson(data);
            problems = LevelDiagnostics.Find(data);
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
            isDirty = dirty;
        }

        private void RecordUndo()
        {
            undoHistory.Add(LevelSerializer.ToJson(level.ToData()));
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

            redoHistory.Add(LevelSerializer.ToJson(level.ToData()));
            level = EditableLevel.From(LevelSerializer.FromJson(Pop(undoHistory)));
            OnLevelChanged();
        }

        private void Redo()
        {
            if (redoHistory.Count == 0)
            {
                return;
            }

            undoHistory.Add(LevelSerializer.ToJson(level.ToData()));
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
            return !isDirty || EditorUtility.DisplayDialog("Level Editor", "The level has unsaved changes. Discard them?", "Discard", "Keep Editing");
        }
    }
}
