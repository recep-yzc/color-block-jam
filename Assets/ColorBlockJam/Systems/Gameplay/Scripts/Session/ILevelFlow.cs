namespace ColorBlockJam.Gameplay
{
    public interface ILevelFlow
    {
        bool IsLeaving { get; }

        void Restart();

        void PlayNext();

        void GoHome();
    }
}
