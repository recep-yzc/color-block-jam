using System;

namespace ColorBlockJam.Gameplay.Logic
{
    public sealed class LevelTimer
    {
        public LevelTimer(float duration)
        {
            Remaining = duration;
        }

        public float Remaining { get; private set; }
        public bool IsPaused { get; set; }
        public bool IsExpired => Remaining <= 0f;

        private float FreezeLeft { get; set; }
        public bool IsFrozen => FreezeLeft > 0f;

        public void Freeze(float seconds)
        {
            FreezeLeft = Math.Max(FreezeLeft, seconds);
        }

        public void Add(float seconds)
        {
            if (seconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Only positive time can be added.");
            }

            Remaining += seconds;
        }

        public bool Tick(float deltaTime)
        {
            if (IsPaused || IsExpired)
            {
                return false;
            }

            var held = Math.Min(FreezeLeft, deltaTime);
            FreezeLeft -= held;
            Remaining = Math.Max(0f, Remaining - (deltaTime - held));
            return IsExpired;
        }
    }
}
