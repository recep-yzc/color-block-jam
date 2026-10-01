using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void DrawInspector()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(InspectorWidth));
            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

            GUILayout.Label("Board", EditorStyles.boldLabel);
            var width = EditorGUILayout.IntSlider(new GUIContent("Width", "Yatayda hücre sayısı."), level.Width, EditableLevel.MinSize, EditableLevel.MaxSize);
            var height = EditorGUILayout.IntSlider(new GUIContent("Height", "Aşağıdan yukarıya hücre sayısı."), level.Height, EditableLevel.MinSize, EditableLevel.MaxSize);
            if (width != level.Width || height != level.Height)
            {
                Change(() => level.Resize(width, height));
                ExitIfLayoutStale();
            }

            var time = EditorGUILayout.IntSlider(new GUIContent("Time (seconds)", "Süre sıfıra inince seviye kaybedilir."), level.TimeLimit, 10, 600);
            if (time != level.TimeLimit)
            {
                Change(() => level.TimeLimit = time);
            }

            var difficulty = (LevelDifficulty)EditorGUILayout.EnumPopup(new GUIContent("Difficulty", "Seviyede oyuncuya gösterilir."), level.Difficulty);
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

            EditorGUILayout.HelpBox(ToolHelp[(int)tool], MessageType.None);

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
                var newColor = EditorGUILayout.Popup("Color", block.Color, ColorNames());
                if (newColor != block.Color)
                {
                    Change(() => block.Color = newColor);
                }

                var axis = (BlockAxis)EditorGUILayout.EnumPopup(new GUIContent("Moves",
                    "Serbest ya da tek eksende: ok bloğu. Üstündeki ok hangi yönde gidebildiğini gösterir."), block.Axis);
                if (axis != block.Axis)
                {
                    Change(() => block.Axis = axis);
                }

                var ice = EditorGUILayout.IntSlider(new GUIContent("Ice",
                        "Blok donmuş başlar ve bu kadar başka blok çıkana kadar hareket edemez. 0 = buz yok."),
                    block.Ice, 0, Mathf.Max(0, level.Blocks.Count - 1));
                if (ice != block.Ice)
                {
                    Change(() => block.Ice = ice);
                }

                if (GUILayout.Button("Delete Block"))
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
                    if (GUI.Button(swatch, new GUIContent(string.Empty, $"{palette.GetName(index)} (tuş {(index + 1) % 10})"), GUIStyle.none))
                    {
                        color = index;
                    }
                }
            }

            GUILayout.Label($"Chosen: {palette.GetName(Mathf.Clamp(color, 0, palette.Count - 1))}", EditorStyles.miniLabel);
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
                    if (GUI.Button(box, GUIContent.none, GUIStyle.none))
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
                    palette.GetColor(color));
            }
        }

        private void DrawCheckSection()
        {
            ExitIfLayoutStale();
            GUILayout.Label("Check", EditorStyles.boldLabel);
            foreach (var problem in problems)
            {
                EditorGUILayout.HelpBox(problem.Block >= 0 ? Describe(problem) + " It is outlined in red." : Describe(problem),
                    MessageType.Error);
            }

            if (validation != null)
            {
                EditorGUILayout.HelpBox("Checking whether the level can be solved…", MessageType.Info);
            }
            else if (result == null || resultRevision != revision)
            {
                EditorGUILayout.HelpBox("Not checked since the last change. Press Check.", MessageType.None);
            }
            else if (result.IsSolved)
            {
                var suggested = SuggestDifficulty(result);
                var moves = LevelRating.MovesToWin(level.Blocks.Count, result);
                EditorGUILayout.HelpBox($"Wins in {moves} moves: {level.Blocks.Count} block(s) to send out, and {result.Repositions} " +
                                        $"to move out of the way first. That plays like {suggested}.", MessageType.Info);
                if (suggested != level.Difficulty && GUILayout.Button($"Set Difficulty To {suggested}"))
                {
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
                EditorGUILayout.HelpBox($"No solution found within {ValidationBudget} positions. The level may be too hard or unsolvable.",
                    MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(validation != null))
            {
                if (GUILayout.Button("Check Level"))
                {
                    StartValidation();
                }
            }
        }

        private void DrawSolutionPreview()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀", GUILayout.Width(30f)))
            {
                SetPreviewStep(previewStep - 1);
            }

            var step = EditorGUILayout.IntSlider(previewStep, 0, result.Moves.Count);
            if (step != previewStep)
            {
                SetPreviewStep(step);
            }

            if (GUILayout.Button("▶", GUILayout.Width(30f)))
            {
                SetPreviewStep(previewStep + 1);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Step through the solution on the board. Step 0 edits again.", EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawGenerateSection()
        {
            GUILayout.Label("Generate", EditorStyles.boldLabel);
            generateDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", generateDifficulty);
            EditorGUILayout.BeginHorizontal();
            generateSeed = EditorGUILayout.IntField(new GUIContent("Seed", "Aynı seed her zaman aynı seviyeyi üretir."), generateSeed);
            if (GUILayout.Button(new GUIContent("🎲", "Rastgele bir seed seçer."), GUILayout.Width(30f)))
            {
                generateSeed = new Random().Next(1, 100000);
            }

            EditorGUILayout.EndHorizontal();
            generateHoles = EditorGUILayout.IntSlider(new GUIContent("Holes", "Tahtanın içine açılacak delik sayısı. Her delik en az 2x2 hücredir."),
                generateHoles, 0, 2);
            if (GUILayout.Button("Generate Level") && ConfirmDiscard())
            {
                Generate();
            }

            EditorGUILayout.LabelField("Makes a new solvable level of that difficulty, replacing the board. Save it to keep it.",
                EditorStyles.wordWrappedMiniLabel);
        }
    }
}
