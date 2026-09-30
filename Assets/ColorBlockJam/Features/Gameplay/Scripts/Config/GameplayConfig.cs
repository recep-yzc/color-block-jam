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
        [Tooltip("How fast a dragged block catches up with the finger. Higher feels tighter, lower feels heavier.")]
        [SerializeField, Min(1f)] private float followSharpness = 26f;
        [Tooltip("Fastest a dragged block moves, in cells per second, so a fling stays under control.")]
        [SerializeField, Min(1f)] private float maxDragSpeed = 40f;
        [Tooltip("Radius, in cells, of the invisible rounding on block corners. A block rolls around a corner on this " +
                 "curve instead of catching on it. 0 = sharp corners.")]
        [SerializeField, Range(0f, 0.45f)] private float cornerRounding = 0.3f;
        [Tooltip("How high a dragged block lifts, in world units.")]
        [SerializeField, Min(0f)] private float liftHeight = 0.35f;
        [SerializeField, Min(0.01f)] private float liftDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float snapDuration = 0.14f;
        [Tooltip("An ease that overshoots would push the block into its neighbor for a moment.")]
        [SerializeField] private Ease snapEase = Ease.OutCubic;

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
        [Tooltip("States searched to find out whether the board can still be cleared.")]
        [SerializeField, Min(100)] private int stuckSearchBudget = 30000;
        [Tooltip("States searched to find the auto play solution.")]
        [SerializeField, Min(100)] private int autoPlaySearchBudget = 60000;
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
        public float FollowSharpness => followSharpness;
        public float MaxDragSpeed => maxDragSpeed;
        public float CornerRounding => cornerRounding;
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
