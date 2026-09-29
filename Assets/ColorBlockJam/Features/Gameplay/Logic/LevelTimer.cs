using System;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// Counts the level time down. It runs only while not paused and stops for good at zero.
    /// </summary>
    public sealed class LevelTimer
    {
        public LevelTimer(float duration)
        {
            Remaining = duration;
        }

        public float Remaining { get; private set; }
        public bool IsPaused { get; set; }
        public bool IsExpired => Remaining <= 0f;

        /// <returns>True on the tick the time runs out.</returns>
        public bool Tick(float deltaTime)
        {
            if (IsPaused || IsExpired)
            {
                return false;
            }

            Remaining = Math.Max(0f, Remaining - deltaTime);
            return IsExpired;
        }
    }
}
