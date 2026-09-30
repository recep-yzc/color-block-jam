namespace ColorBlockJam.Gameplay
{
    public interface ILevelFlow
    {
        /// <summary>Plays the same level again from its start state.</summary>
        void Restart();

        /// <summary>Plays the player's current level, which is the next one after a win.</summary>
        void PlayNext();

        void GoHome();
    }
}
