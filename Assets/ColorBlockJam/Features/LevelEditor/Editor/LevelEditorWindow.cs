using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class LevelEditorWindow : EditorWindow
    {
        private enum Tool
        {
            Draw,
            Stamp,
            Door,
            Move,
            Erase
        }

        private readonly struct Hit
        {
            public readonly bool IsCell;
            public readonly bool IsSlot;
            public readonly GridPoint Cell;
            public readonly BoardSide Side;
            public readonly int Slot;

            public Hit(GridPoint cell)
            {
                IsCell = true;
                IsSlot = false;
                Cell = cell;
                Side = default;
                Slot = 0;
            }

            public Hit(BoardSide side, int slot)
            {
                IsCell = false;
                IsSlot = true;
                Cell = default;
                Side = side;
                Slot = slot;
            }
        }

        private const float SidebarWidth = 200f;
        private const float InspectorWidth = 280f;
        private const int ValidationBudget = 100000;
        private const int HistoryLimit = 100;

        private static readonly string[] ToolNames = { "Draw", "Stamp", "Door", "Move", "Erase" };
        private static readonly string[] ToolHelp =
        {
            "Drag over empty cells to draw one block of the chosen color. Click a block to select it.",
            "Click a cell to place the chosen shape in the chosen color.",
            "Click or drag along the walls to place doors of the chosen color. A block leaves through a door of its own color.",
            "Drag a block to move it.",
            "Click a block or a door to remove it. Right-click erases with every tool."
        };

        private static readonly Color SelectedOutline = new(1f, 1f, 1f, 0.9f);
        private static readonly Color ProblemOutline = new(1f, 0.25f, 0.25f, 1f);
        private static readonly Color Background = new(0.13f, 0.16f, 0.27f);
        private static readonly Color CellColor = new(0.24f, 0.33f, 0.64f);
        private static readonly Color WallColor = new(0.78f, 0.81f, 0.89f);
        private static readonly Color ArrowColor = new(1f, 0.96f, 0.9f, 0.95f);
        private static readonly Color IceTint = new(0.8f, 0.93f, 1f, 0.55f);
        private static readonly Color IceCountColor = new(0.1f, 0.24f, 0.45f);

        [SerializeField] private string levelJson;
        [SerializeField] private int catalogIndex = -1;
        [SerializeField] private bool isDirty;
        [SerializeField] private Tool tool;
        [SerializeField] private int color;
        [SerializeField] private int shapeIndex;
        [SerializeField] private LevelDifficulty generateDifficulty = LevelDifficulty.Medium;
        [SerializeField] private int generateSeed = 1;

        private readonly List<string> undoHistory = new();
        private readonly List<string> redoHistory = new();
        private readonly List<string> levelSummaries = new();
        private LevelCatalog catalog;
        private BlockPalette palette;
        private EditableLevel level;
        private List<GridPoint[]> shapes;
        private List<LevelProblem> problems = new();

        private readonly HashSet<int> problemBlocks = new();
        private GUIStyle iceCountStyle;
        private int selectedBlock = -1;

        private EditableBlock drawing;
        private int movingBlock = -1;
        private GridPoint moveGrab;
        private GridPoint moveOffset;
        private bool hasHover;
        private Hit hover;
        private int paintValue;

        private Rect boardRect;
        private float cellSize;

        private int revision;
        private Task<SolveResult> validation;
        private int validationRevision;
        private SolveResult result;
        private int resultRevision = -1;
        private int previewStep;
        private Board preview;

        private Vector2 sidebarScroll;
        private Vector2 inspectorScroll;

        private bool isLayoutStale;

        [MenuItem("Color Block Jam/Level Editor", priority = 0)]
        public static LevelEditorWindow Open()
        {
            var window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(900f, 560f);
            window.Show();
            return window;
        }

        public void Load(LevelCatalog source, int index)
        {
            if (!ConfirmDiscard())
            {
                return;
            }

            catalog = source;
            catalogIndex = index;
            SetLevel(EditableLevel.From(LevelSerializer.FromJson(source.Levels[index].text)), dirty: false);
            ResetHistory();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            shapes = new List<GridPoint[]>();
            shapes.AddRange(BlockShapes.Small);
            shapes.AddRange(BlockShapes.Long);
            shapes.AddRange(BlockShapes.Complex);
            catalog = FindAsset<LevelCatalog>();
            palette = FindAsset<BlockPalette>();

            var restored = string.IsNullOrEmpty(levelJson) ? null : LevelSerializer.FromJson(levelJson);
            level = restored != null ? EditableLevel.From(restored) : NewLevel();
            if (restored == null && catalog != null && catalog.Count > 0)
            {
                catalogIndex = 0;
                level = EditableLevel.From(LevelSerializer.FromJson(catalog.Levels[0].text));
            }

            var wasDirty = isDirty && restored != null;
            OnLevelChanged();
            isDirty = wasDirty;
            isLayoutStale = false;
            RefreshSummaries();
        }

        private void Update()
        {
            if (validation == null || !validation.IsCompleted)
            {
                return;
            }

            if (validation.IsCompletedSuccessfully && validationRevision == revision)
            {
                result = validation.Result;
                resultRevision = revision;
            }

            validation = null;
            Repaint();
        }

        private void OnGUI()
        {
            if (palette == null || catalog == null)
            {
                EditorGUILayout.HelpBox("A BlockPalette and a LevelCatalog asset are needed. Create them from Assets > Create > Color Block Jam > Level.", MessageType.Error);
                return;
            }

            HandleShortcuts();
            ExitIfLayoutStale();
            DrawToolbar();
            ExitIfLayoutStale();
            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            ExitIfLayoutStale();
            DrawBoardArea();
            ExitIfLayoutStale();
            DrawInspector();
            EditorGUILayout.EndHorizontal();
            DrawStatusBar();
        }

        private void ExitIfLayoutStale()
        {
            if (!isLayoutStale)
            {
                return;
            }

            isLayoutStale = false;
            if (Event.current != null && Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
            {
                GUIUtility.ExitGUI();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var title = catalogIndex >= 0 && catalogIndex < catalog.Count ? $"Level {catalogIndex + 1}  ({catalog.Levels[catalogIndex].name})" : "New level";
            GUILayout.Label(isDirty ? title + "  *" : title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("New", "Boş bir seviye başlatır."), EditorStyles.toolbarButton) && ConfirmDiscard())
            {
                catalogIndex = -1;
                SetLevel(NewLevel(), dirty: false);
                ResetHistory();
            }

            using (new EditorGUI.DisabledScope(catalogIndex < 0))
            {
                if (GUILayout.Button(new GUIContent("Save", "Seviyenin dosyasının üzerine kaydeder."), EditorStyles.toolbarButton))
                {
                    Save(asNew: false);
                }
            }

            if (GUILayout.Button(new GUIContent("Save As New Level", "Kataloğun sonuna yeni bir dosya olarak kaydeder."), EditorStyles.toolbarButton))
            {
                Save(asNew: true);
            }

            GUILayout.Space(12f);
            using (new EditorGUI.DisabledScope(undoHistory.Count == 0))
            {
                if (GUILayout.Button(new GUIContent("Undo", "Geri alır (Ctrl+Z)."), EditorStyles.toolbarButton))
                {
                    Undo();
                }
            }

            using (new EditorGUI.DisabledScope(redoHistory.Count == 0))
            {
                if (GUILayout.Button(new GUIContent("Redo", "Yineler (Ctrl+Y)."), EditorStyles.toolbarButton))
                {
                    Redo();
                }
            }

            GUILayout.Space(12f);
            if (GUILayout.Button(new GUIContent("Check", "Seviyenin çözülebilir olup olmadığını bulur."), EditorStyles.toolbarButton))
            {
                StartValidation();
            }

            if (GUILayout.Button(new GUIContent("▶ Play", "Bu seviyeyi oyun sahnesinde oynatır."), EditorStyles.toolbarButton))
            {
                LevelTestPlay.Play(level.ToData());
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(SidebarWidth));
            GUILayout.Label("Levels", EditorStyles.boldLabel);
            sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);
            for (var i = 0; i < catalog.Count; i++)
            {
                var isCurrent = i == catalogIndex;
                var style = isCurrent ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                var previous = GUI.backgroundColor;
                GUI.backgroundColor = isCurrent ? new Color(0.55f, 0.8f, 1f) : previous;
                if (GUILayout.Button(i < levelSummaries.Count ? levelSummaries[i] : $"{i + 1}", style, GUILayout.Height(24f)) && !isCurrent)
                {
                    Load(catalog, i);
                }

                GUI.backgroundColor = previous;
            }

            EditorGUILayout.EndScrollView();

            using (new EditorGUI.DisabledScope(catalogIndex < 0))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("▲", "Seviyeyi katalogda bir öne alır.")))
                {
                    MoveInCatalog(-1);
                }

                if (GUILayout.Button(new GUIContent("▼", "Seviyeyi katalogda bir sonraya alır.")))
                {
                    MoveInCatalog(1);
                }

                if (GUILayout.Button(new GUIContent("Remove", "Seviyeyi katalogdan çıkarır. Dosyası silinmez.")))
                {
                    RemoveFromCatalog();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBoardArea()
        {
            var area = GUILayoutUtility.GetRect(200f, 200f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(area, Background);

            cellSize = Mathf.Floor(Mathf.Min(area.width / (level.Width + 2), area.height / (level.Height + 2)));
            cellSize = Mathf.Clamp(cellSize, 12f, 72f);
            var size = new Vector2((level.Width + 2) * cellSize, (level.Height + 2) * cellSize);
            boardRect = new Rect(area.center - size * 0.5f, size);

            HandleBoardInput(area);

            if (Event.current.type == EventType.Repaint)
            {
                DrawBoard();
            }

            if (preview != null)
            {
                GUI.Label(new Rect(area.x + 8f, area.y + 6f, 400f, 20f), $"Solution preview: step {previewStep} of {result.Moves.Count}. Editing is paused.",
                    EditorStyles.whiteBoldLabel);
            }
        }

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
                EditorGUILayout.HelpBox($"Solvable in {result.Moves.Count} moves. {result.Repositions} block(s) must be moved out of the way first. " +
                                        $"That plays like {suggested}.", MessageType.Info);
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
            if (GUILayout.Button("Generate Level") && ConfirmDiscard())
            {
                Generate();
            }

            EditorGUILayout.LabelField("Makes a new solvable level of that difficulty, replacing the board. Save it to keep it.",
                EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            var colors = new HashSet<int>();
            foreach (var block in level.Blocks)
            {
                colors.Add(block.Color);
            }

            var where = !hasHover ? "" : hover.IsCell ? $"Cell {hover.Cell.X}, {hover.Cell.Y}" : $"{hover.Side} wall, slot {hover.Slot}";
            GUILayout.Label($"{level.Width} × {level.Height}   {level.Blocks.Count} blocks   {colors.Count} colors   {where}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Right-click erases · 1–0 pick a color · Delete removes the selected block", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBoard()
        {
            for (var x = 0; x < level.Width; x++)
            {
                for (var y = 0; y < level.Height; y++)
                {
                    EditorGUI.DrawRect(Shrink(CellRect(x, y), 1f), CellColor);
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

        private void HandleBoardInput(Rect area)
        {
            var current = Event.current;
            var id = GUIUtility.GetControlID(FocusType.Passive);
            var hit = HitTest(current.mousePosition, out var hitResult) && area.Contains(current.mousePosition);

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
                case EventType.MouseDown when hit:
                    if (current.button == 1)
                    {
                        Erase(hitResult);
                    }
                    else if (current.button == 0)
                    {
                        PointerDown(hitResult);
                        GUIUtility.hotControl = id;
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

                    RecordUndo();
                    drawing = new EditableBlock(color);
                    drawing.Cells.Add(hit.Cell);
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
            }
        }

        private void PointerDrag(Hit hit)
        {
            switch (tool)
            {
                case Tool.Draw when drawing != null && hit.IsCell:
                    if (level.BlockAt(hit.Cell) < 0 && IsNextTo(drawing.Cells, hit.Cell))
                    {
                        drawing.Cells.Add(hit.Cell);
                        OnLevelChanged();
                    }

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
            else if (!command && GUIUtility.keyboardControl == 0 && current.keyCode >= KeyCode.Alpha0 && current.keyCode <= KeyCode.Alpha9)
            {
                var index = ((int)current.keyCode - (int)KeyCode.Alpha0 + 9) % 10;
                if (index < palette.Count)
                {
                    color = index;
                    current.Use();
                    Repaint();
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

        private static Rect Shrink(Rect rect, float amount) => new(rect.x + amount, rect.y + amount, rect.width - amount * 2f, rect.height - amount * 2f);

        private static Rect Grow(Rect rect, float amount) => Shrink(rect, -amount);

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

        private void StartValidation()
        {
            if (validation != null)
            {
                return;
            }

            StopPreview();
            var board = BoardFactory.Create(level.ToData());
            validationRevision = revision;
            validation = Task.Run(() => new BoardSolver().Solve(board, ValidationBudget));
            isLayoutStale = true;
        }

        private void SetPreviewStep(int step)
        {
            if (result == null || !result.IsSolved)
            {
                return;
            }

            previewStep = Mathf.Clamp(step, 0, result.Moves.Count);
            isLayoutStale = true;
            if (previewStep == 0)
            {
                preview = null;
                Repaint();
                return;
            }

            preview = BoardFactory.Create(level.ToData());
            for (var i = 0; i < previewStep; i++)
            {
                var move = result.Moves[i];
                var block = preview.Blocks[move.BlockId];
                if (move.Exits)
                {
                    preview.Clear(block);
                }
                else
                {
                    preview.Move(block, move.Target);
                }
            }

            Repaint();
        }

        private void StopPreview()
        {
            preview = null;
            previewStep = 0;
        }

        private void Generate()
        {
            var generator = new LevelGenerator();
            GeneratedLevel generated;
            try
            {
                generated = generator.Generate(generateDifficulty, palette.Count, generateSeed, attempt =>
                    !EditorUtility.DisplayCancelableProgressBar("Level Editor", $"Looking for a {generateDifficulty} level… try {attempt}",
                        attempt / (float)generator.MaxAttempts));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (generated == null)
            {
                EditorUtility.DisplayDialog("Level Editor", "No level was found with this seed. Try another seed.", "OK");
                return;
            }

            RecordUndo();
            SetLevel(EditableLevel.From(generated.Level), dirty: true);
            result = generated.Solution;
            resultRevision = revision;
            generateSeed++;
        }

        private static LevelDifficulty SuggestDifficulty(SolveResult solution)
        {
            return solution.Repositions switch
            {
                0 => LevelDifficulty.Easy,
                1 => LevelDifficulty.Medium,
                _ => LevelDifficulty.Hard
            };
        }

        private void Save(bool asNew)
        {
            var data = level.ToData();
            var isChecked = result != null && resultRevision == revision && result.IsSolved;
            if ((problems.Count > 0 || !isChecked) && !EditorUtility.DisplayDialog("Level Editor",
                    problems.Count > 0 ? "The level has problems (see Check). Save anyway?" : "The level has not been checked as solvable. Save anyway?",
                    "Save Anyway", "Cancel"))
            {
                return;
            }

            string path;
            if (asNew || catalogIndex < 0)
            {
                path = NextLevelPath();
            }
            else
            {
                path = AssetDatabase.GetAssetPath(catalog.Levels[catalogIndex]);
            }

            File.WriteAllText(path, LevelSerializer.ToJson(data));
            AssetDatabase.ImportAsset(path);

            if (asNew || catalogIndex < 0)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                var serializedCatalog = new SerializedObject(catalog);
                var levels = serializedCatalog.FindProperty("levels");
                levels.arraySize++;
                levels.GetArrayElementAtIndex(levels.arraySize - 1).objectReferenceValue = asset;
                serializedCatalog.ApplyModifiedProperties();
                catalogIndex = catalog.Count - 1;
            }

            AssetDatabase.SaveAssets();
            isDirty = false;
            RefreshSummaries();
            ShowNotification(new GUIContent($"Saved {Path.GetFileName(path)}"));
        }

        private string NextLevelPath()
        {
            var folder = catalog.Count > 0
                ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(catalog.Levels[0]))!.Replace('\\', '/')
                : Path.GetDirectoryName(AssetDatabase.GetAssetPath(catalog))!.Replace('\\', '/');
            for (var number = catalog.Count + 1; ; number++)
            {
                var path = $"{folder}/Level{number:00}.json";
                if (!File.Exists(path))
                {
                    return path;
                }
            }
        }

        private void MoveInCatalog(int direction)
        {
            var target = catalogIndex + direction;
            if (target < 0 || target >= catalog.Count)
            {
                return;
            }

            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("levels").MoveArrayElement(catalogIndex, target);
            serializedCatalog.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            catalogIndex = target;
            RefreshSummaries();
        }

        private void RemoveFromCatalog()
        {
            if (!EditorUtility.DisplayDialog("Level Editor", $"Take level {catalogIndex + 1} out of the catalog? Its file stays in the project.",
                    "Remove", "Cancel"))
            {
                return;
            }

            var serializedCatalog = new SerializedObject(catalog);
            var levels = serializedCatalog.FindProperty("levels");
            levels.GetArrayElementAtIndex(catalogIndex).objectReferenceValue = null;
            levels.DeleteArrayElementAtIndex(catalogIndex);
            serializedCatalog.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            catalogIndex = -1;
            isDirty = true;
            RefreshSummaries();
        }

        private void RefreshSummaries()
        {
            isLayoutStale = true;
            levelSummaries.Clear();
            if (catalog == null)
            {
                return;
            }

            for (var i = 0; i < catalog.Count; i++)
            {
                var text = catalog.Levels[i];
                if (text == null)
                {
                    levelSummaries.Add($"{i + 1}. (missing)");
                    continue;
                }

                var data = LevelSerializer.FromJson(text.text);
                levelSummaries.Add($"{i + 1}. {text.name}  ·  {data.difficulty}");
            }
        }

        private string[] ColorNames()
        {
            var names = new string[palette.Count];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = palette.GetName(i);
            }

            return names;
        }

        private string Describe(LevelProblem problem)
        {
            var colorName = problem.Color >= 0 && problem.Color < palette.Count ? palette.GetName(problem.Color) : "?";
            return problem.Kind switch
            {
                LevelProblemKind.NoBlocks => "The board has no blocks.",
                LevelProblemKind.BlockOutsideBoard => $"A {colorName} block is outside the board.",
                LevelProblemKind.BlocksOverlap => $"A {colorName} block overlaps another block.",
                LevelProblemKind.DoorOutsideBoard => $"A {colorName} door is off the edge.",
                LevelProblemKind.DoorsOverlap => $"Two doors overlap ({colorName}).",
                LevelProblemKind.ColorHasNoDoor => $"{colorName} blocks have no {colorName} door to leave through.",
                LevelProblemKind.BlockFitsNoDoor => $"A {colorName} block fits no {colorName} door it can reach. An arrow " +
                                                    "block reaches only the doors ahead of it along its arrow.",
                LevelProblemKind.IceNeverMelts => $"A {colorName} block has more ice than there are other blocks to melt it.",
                _ => problem.Kind.ToString()
            };
        }

        private static EditableLevel NewLevel() => new(LevelData.DefaultWidth, LevelData.DefaultHeight);

        private static T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
