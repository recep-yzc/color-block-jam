namespace ColorBlockJam.Shared.Navigation
{
    /// <summary>
    /// A page or a tab that is identified and sorted by its page id.
    /// </summary>
    public interface INavigationItem
    {
        string PageId { get; }
    }
}
