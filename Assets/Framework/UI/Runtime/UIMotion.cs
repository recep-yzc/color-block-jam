using LitMotion;

namespace Framework.UI
{
    public static class UIMotion
    {
        /// <summary>
        /// UI keeps animating while the game is paused with <c>Time.timeScale = 0</c>.
        /// </summary>
        public static readonly IMotionScheduler Scheduler = MotionScheduler.UpdateIgnoreTimeScale;
    }
}
