namespace ColorBlockJam.Gameplay.Logic
{
    public readonly struct ArrowRun
    {
        public ArrowRun(float centerX, float centerY, int length)
        {
            CenterX = centerX;
            CenterY = centerY;
            Length = length;
        }

        public float CenterX { get; }
        public float CenterY { get; }

        public int Length { get; }
    }
}
