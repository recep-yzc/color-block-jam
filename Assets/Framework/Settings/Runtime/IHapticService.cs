namespace Framework.Settings
{
    public interface IHapticService
    {
        /// <summary>Vibrates the device once, if the player has haptics on.</summary>
        void Play();
    }
}
