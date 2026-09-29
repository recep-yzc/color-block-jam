using LitMotion;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Every tuning value of the gameplay scene: how blocks feel under the finger, animation timing,
    /// search budgets and camera framing.
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Gameplay/Gameplay Config", fileName = "GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Board")]
        [Tooltip("World size of one board cell. The art is made for 2.")]
        [SerializeField, Min(0.1f)] private float cellSize = 2f;

        [Header("Drag")]
        [Tooltip("How far out of line, in cells, a dragged block may be to still be eased around a corner.")]
        [SerializeField, Range(0f, 0.5f)] private float cornerAssist = 0.4f;
        [Tooltip("Cells of easing per cell of movement. Higher rounds corners more tightly.")]
        [SerializeField, Min(0f)] private float cornerAssistRate = 1.5f;
        [Tooltip("How fast the block's look catches up with its logical place. Higher feels stiffer.")]
        [SerializeField, Min(1f)] private float followSharpness = 22f;
        [Tooltip("How high a dragged block lifts, in world units.")]
        [SerializeField, Min(0f)] private float liftHeight = 0.35f;
        [SerializeField, Min(0.01f)] private float liftDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float snapDuration = 0.14f;
        [SerializeField] private Ease snapEase = Ease.OutBack;

        [Header("Exit")]
        [Tooltip("How far, in cells, a block must be pushed through its door to leave.")]
        [SerializeField, Range(0.05f, 1f)] private float exitDepth = 0.3f;
        [Tooltip("Speed of a leaving block, in cells per second.")]
        [SerializeField, Min(0.1f)] private float exitSpeed = 12f;
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField, Min(0)] private int burstPrewarm = 4;
        [Tooltip("Seconds between the level ending and its result popup, so the last moves can finish.")]
        [SerializeField, Min(0f)] private float resultPopupDelay = 0.6f;

        [Header("Solver")]
        [Tooltip("States searched after each move to find out whether the board is stuck.")]
        [SerializeField, Min(100)] private int stuckSearchBudget = 20000;
        [Tooltip("States searched to find the auto play solution.")]
        [SerializeField, Min(100)] private int autoPlaySearchBudget = 400000;
        [Tooltip("Seconds per cell of an auto play slide.")]
        [SerializeField, Min(0.01f)] private float autoPlayCellDuration = 0.08f;
        [SerializeField, Min(0f)] private float autoPlayPause = 0.12f;

        [Header("Timer")]
        [Tooltip("The timer turns to its warning look below this many seconds.")]
        [SerializeField, Min(0)] private int timerWarningSeconds = 10;

        [Header("Camera")]
        [Tooltip("Camera pitch in degrees; 90 looks straight down.")]
        [SerializeField, Range(30f, 90f)] private float cameraPitch = 62f;
        [Tooltip("Part of the screen height the board may use, leaving room for the HUD.")]
        [SerializeField, Range(0.3f, 1f)] private float boardScreenHeight = 0.62f;
        [Tooltip("Part of the screen width the board may use.")]
        [SerializeField, Range(0.3f, 1f)] private float boardScreenWidth = 0.92f;

        [Header("Views")]
        [SerializeField] private BlockView blockViewPrefab;

        public float CellSize => cellSize;
        public float CornerAssist => cornerAssist;
        public float CornerAssistRate => cornerAssistRate;
        public float FollowSharpness => followSharpness;
        public float LiftHeight => liftHeight;
        public float LiftDuration => liftDuration;
        public float SnapDuration => snapDuration;
        public Ease SnapEase => snapEase;
        public float ExitDepth => exitDepth;
        public float ExitSpeed => exitSpeed;
        public ParticleSystem BurstPrefab => burstPrefab;
        public int BurstPrewarm => burstPrewarm;
        public float ResultPopupDelay => resultPopupDelay;
        public int StuckSearchBudget => stuckSearchBudget;
        public int AutoPlaySearchBudget => autoPlaySearchBudget;
        public float AutoPlayCellDuration => autoPlayCellDuration;
        public float AutoPlayPause => autoPlayPause;
        public int TimerWarningSeconds => timerWarningSeconds;
        public float CameraPitch => cameraPitch;
        public float BoardScreenHeight => boardScreenHeight;
        public float BoardScreenWidth => boardScreenWidth;
        public BlockView BlockViewPrefab => blockViewPrefab;
    }
}
