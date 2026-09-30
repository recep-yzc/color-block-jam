using System;

namespace ColorBlockJam.Gameplay.Logic
{
    /// <summary>
    /// Counts the level time down. It runs only while not paused and stops for good at zero.
    /// A freeze holds it for a while without pausing the level, as the freeze booster does.
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

        /// <summary>Seconds the timer stays held by a freeze.</summary>
        public float FreezeLeft { get; private set; }
        public bool IsFrozen => FreezeLeft > 0f;

        /// <summary>Holds the timer for <paramref name="seconds"/> of play; a longer freeze replaces a shorter one.</summary>
        public void Freeze(float seconds)
        {
            FreezeLeft = Math.Max(FreezeLeft, seconds);
        }

        /// <returns>True on the tick the time runs out.</returns>
        public bool Tick(float deltaTime)
        {
            if (IsPaused || IsExpired)
            {
                return false;
            }

            // A freeze uses up the time first; whatever is left of the step counts down.
            var held = Math.Min(FreezeLeft, deltaTime);
            FreezeLeft -= held;
            Remaining = Math.Max(0f, Remaining - (deltaTime - held));
            return IsExpired;
        }
    }
}
