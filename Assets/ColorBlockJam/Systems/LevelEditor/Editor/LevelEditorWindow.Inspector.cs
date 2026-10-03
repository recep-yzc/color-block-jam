using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using Random = System.Random;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void DrawInspector()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(InspectorWidth));
            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

            GUILayout.Label("Board", EditorStyles.boldLabel);
            var width = EditorGUILayout.IntSlider(LevelEditorLabels.Width, level.Width, EditableLevel.MinSize, EditableLevel.MaxSize);
            var height = EditorGUILayout.IntSlider(LevelEditorLabels.Height, level.Height, EditableLevel.MinSize, EditableLevel.MaxSize);
            if (width != level.Width || height != level.Height)
            {
                Change(() => level.Resize(width, height));
                ExitIfLayoutStale();
            }

            var time = EditorGUILayout.IntSlider(LevelEditorLabels.Time, level.TimeLimit, 10, 600);
            if (time != level.TimeLimit)
            {
                Change(() => level.TimeLimit = time);
            }

            var difficulty = (LevelDifficulty)EditorGUILayout.EnumPopup(LevelEditorLabels.Difficulty, level.Difficulty);
            if (difficulty != level.Difficulty)
            {
                Change(() => level.Difficulty = difficulty);
            }

            ExitIfLayoutStale();

            EditorGUILayout.Space();
            GUILayout.Label("Tool", EditorStyles.boldLabel);
            var chosenTool = (Tool)GUILayout.Toolbar((int)tool, ToolNames);
            if (chosenTool != tool)
            {
                tool = chosenTool;
                isLayoutStale = true;
                ExitIfLayoutStale();
            }

            EditorGUILayout.HelpBox(HelpFor(tool), MessageType.None);

            EditorGUILayout.Space();
            GUILayout.Label("Color", EditorStyles.boldLabel);
            DrawColorPicker();

            if (tool == Tool.Stamp)
            {
                EditorGUILayout.Space();
                GUILayout.Label("Shape", EditorStyles.boldLabel);
                DrawShapePicker();
            }

            ExitIfLayoutStale();
            if (selectedBlock >= 0 && selectedBlock < level.Blocks.Count)
            {
                EditorGUILayout.Space();
                GUILayout.Label("Selected Block", EditorStyles.boldLabel);
                var block = level.Blocks[selectedBlock];
                var newColor = EditorGUILayout.Popup(LevelEditorLabels.BlockColor, block.Color, colorNames);
                if (newColor != block.Color)
                {
                    Change(() => block.Color = newColor);
                }

                var axis = (BlockAxis)EditorGUILayout.EnumPopup(LevelEditorLabels.Moves, block.Axis);
                if (axis != block.Axis)
                {
                    Change(() => block.Axis = axis);
                }

                var ice = EditorGUILayout.IntSlider(LevelEditorLabels.Ice, block.Ice, 0, Mathf.Max(0, level.Blocks.Count - 1));
                if (ice != block.Ice)
                {
                    Change(() => block.Ice = ice);
                }

                if (GUILayout.Button(LevelEditorLabels.DeleteBlock))
                {
                    var index = selectedBlock;
                    selectedBlock = -1;
                    Change(() => level.Blocks.RemoveAt(index));
                }

                ExitIfLayoutStale();
            }

            EditorGUILayout.Space();
            DrawCheckSection();
            ExitIfLayoutStale();

            EditorGUILayout.Space();
            DrawGenerateSection();
            ExitIfLayoutStale();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private static string HelpFor(Tool chosen)
        {
            foreach (var entry in Tools)
            {
                if (entry.Tool == chosen)
                {
                    return entry.Help;
                }
            }

            return string.Empty;
        }

        private void DrawColorPicker()
        {
            const int perRow = 5;
            for (var row = 0; row * perRow < palette.Count; row++)
            {
                var rect = GUILayoutUtility.GetRect(InspectorWidth - 20f, 34f);
                var swatchWidth = rect.width / perRow;
                for (var column = 0; column < perRow; column++)
                {
                    var index = row * perRow + column;
                    if (index >= palette.Count)
                    {
                        break;
                    }

                    var swatch = new Rect(rect.x + column * swatchWidth + 2f, rect.y + 2f, swatchWidth - 4f, rect.height - 4f);
                    if (index == color)
                    {
                        EditorGUI.DrawRect(Grow(swatch, 2f), Color.white);
                    }

                    EditorGUI.DrawRect(swatch, palette.GetColor(index));
                    if (GUI.Button(swatch, swatchLabels[index], GUIStyle.none))
                    {
                        color = index;
                    }
                }
            }

            GUILayout.Label(chosenLabels[Mathf.Clamp(color, 0, chosenLabels.Length - 1)], EditorStyles.miniLabel);
        }

        private void DrawShapePicker()
        {
            const int perRow = 4;
            for (var row = 0; row * perRow < shapes.Count; row++)
            {
                var rect = GUILayoutUtility.GetRect(InspectorWidth - 20f, 58f);
                var boxWidth = rect.width / perRow;
                for (var column = 0; column < perRow; column++)
                {
                    var index = row * perRow + column;
                    if (index >= shapes.Count)
                    {
                        break;
                    }

                    var box = new Rect(rect.x + column * boxWidth + 2f, rect.y + 2f, boxWidth - 4f, rect.height - 4f);
                    EditorGUI.DrawRect(box, index == shapeIndex ? new Color(0.35f, 0.55f, 0.9f) : new Color(0.22f, 0.22f, 0.22f));
                    DrawShapeThumbnail(shapes[index], box);
                    if (GUI.Button(box, LevelEditorLabels.Shape, GUIStyle.none))
                    {
                        shapeIndex = index;
                    }
                }
            }
        }

        private void DrawShapeThumbnail(GridPoint[] shape, Rect box)
        {
            var maxX = 0;
            var maxY = 0;
            foreach (var cell in shape)
            {
                maxX = Mathf.Max(maxX, cell.X);
                maxY = Mathf.Max(maxY, cell.Y);
            }

            var unit = Mathf.Floor(Mathf.Min((box.width - 8f) / (maxX + 1), (box.height - 8f) / (maxY + 1)));
            var origin = new Vector2(box.center.x - (maxX + 1) * unit * 0.5f, box.center.y + (maxY + 1) * unit * 0.5f);
            foreach (var cell in shape)
            {
                EditorGUI.DrawRect(new Rect(origin.x + cell.X * unit + 1f, origin.y - (cell.Y + 1) * unit + 1f, unit - 2f, unit - 2f),
                    ColorOf(color));
            }
        }

        private void DrawCheckSection()
        {
            GUILayout.Label("Check", EditorStyles.boldLabel);
            foreach (var text in problemTexts)
            {
                EditorGUILayout.HelpBox(text, MessageType.Error);
            }

            if (validation != null)
            {
                EditorGUILayout.HelpBox("Checking whether the level can be solved…", MessageType.Info);
            }
            else if (validationError != null)
            {
                EditorGUILayout.HelpBox($"The check stopped with an error: {validationError}", MessageType.Error);
            }
            else if (result == null || resultRevision != revision)
            {
                EditorGUILayout.HelpBox("Not checked since the last change. Press Check.", MessageType.None);
            }
            else if (result.IsSolved)
            {
                EditorGUILayout.HelpBox(resultSummary, MessageType.Info);
                if (suggestedDifficulty != level.Difficulty && GUILayout.Button(setDifficultyLabel))
                {
                    var suggested = suggestedDifficulty;
                    Change(() => level.Difficulty = suggested);
                }

                DrawSolutionPreview();
            }
            else if (result.IsStuck)
            {
                EditorGUILayout.HelpBox("This level cannot be solved: no order of moves clears the board. The player will get the stuck popup.",
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox($"No solution found within {BoardSolver.DefaultBudget} positions, the same budget the game's stuck " +
                                        "check uses. The level may be too hard or unsolvable.",
                    MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(validation != null))
            {
                if (GUILayout.Button(LevelEditorLabels.CheckLevel))
                {
                    StartValidation();
                }
            }
        }

        private void DrawSolutionPreview()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(LevelEditorLabels.StepBack, GUILayout.Width(30f)))
            {
                SetPreviewStep(previewStep - 1);
            }

            var step = EditorGUILayout.IntSlider(previewStep, 0, result.Moves.Count);
            if (step != previewStep)
            {
                SetPreviewStep(step);
            }

            if (GUILayout.Button(LevelEditorLabels.StepForward, GUILayout.Width(30f)))
            {
                SetPreviewStep(previewStep + 1);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Step through the solution on the board. Step 0 edits again.", EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawGenerateSection()
        {
            GUILayout.Label("Generate", EditorStyles.boldLabel);
            generateDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup(LevelEditorLabels.GenerateDifficulty, generateDifficulty);
            EditorGUILayout.BeginHorizontal();
            generateSeed = EditorGUILayout.IntField(LevelEditorLabels.Seed, generateSeed);
            if (GUILayout.Button(LevelEditorLabels.RandomSeed, GUILayout.Width(30f)))
            {
                generateSeed = new Random().Next(1, 100000);
            }

            EditorGUILayout.EndHorizontal();
            generateHoles = EditorGUILayout.IntSlider(LevelEditorLabels.Holes, generateHoles, 0, 2);
            using (new EditorGUI.DisabledScope(presets == null))
            {
                if (GUILayout.Button(LevelEditorLabels.EditPresets))
                {
                    Selection.activeObject = presets;
                    EditorGUIUtility.PingObject(presets);
                }
            }

            if (GUILayout.Button(LevelEditorLabels.GenerateLevel) && ConfirmDiscard())
            {
                Generate();
            }

            EditorGUILayout.LabelField("Makes a new solvable level of that difficulty, replacing the board. It is a new level: Save As New Level keeps it.",
                EditorStyles.wordWrappedMiniLabel);
        }
    }
}
