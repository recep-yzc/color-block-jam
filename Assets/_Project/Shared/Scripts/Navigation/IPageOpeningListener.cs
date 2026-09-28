namespace ColorBlockJam.Shared.Navigation
{
    /// <summary>
    /// Put on any component under a <see cref="NavigationPage"/> to refresh it
    /// each time the page starts to open.
    /// </summary>
    public interface IPageOpeningListener
    {
        void OnPageOpening();
    }
}
