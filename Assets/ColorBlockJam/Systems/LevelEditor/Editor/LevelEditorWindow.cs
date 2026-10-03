using System.Collections.Generic;
using System.Threading.Tasks;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow : EditorWindow
    {
        private enum Tool
        {
            Draw,
            Stamp,
            Door,
            Move,
            Erase,
            Hole
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
        private const int HoleSizeLimit = 3;

        private static readonly string[] ToolNames = { "Draw", "Stamp", "Door", "Move", "Erase", "Hole" };
        private static readonly string[] ToolHelp =
        {
            "Drag over empty cells to draw one block of the chosen color. Click a block to select it.",
            "Click a cell to place the chosen shape in the chosen color.",
            "Click or drag along the walls to place doors of the chosen color. A block leaves through a door of its own color.",
            "Drag a block to move it.",
            "Click a block or a door to remove it, or a removed cell to put it back. Right-click erases with every tool.",
            "Click or drag over empty cells to remove them from the board, and again to put them back. In the game a " +
            "removed cell is a hole with a wall around it."
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
        [SerializeField] private int generateHoles;

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
        private bool paintHole;

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
    }
}
