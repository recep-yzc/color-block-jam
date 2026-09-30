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

        public float FreezeLeft { get; private set; }
        public bool IsFrozen => FreezeLeft > 0f;

        public void Freeze(float seconds)
        {
            FreezeLeft = Math.Max(FreezeLeft, seconds);
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
