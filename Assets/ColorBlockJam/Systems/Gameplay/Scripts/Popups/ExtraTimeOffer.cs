namespace ColorBlockJam.Gameplay
{
    public readonly struct ExtraTimeOffer
    {
        public readonly int Seconds;
        public readonly int Cost;

        public ExtraTimeOffer(int seconds, int cost)
        {
            Seconds = seconds;
            Cost = cost;
        }
    }
}
