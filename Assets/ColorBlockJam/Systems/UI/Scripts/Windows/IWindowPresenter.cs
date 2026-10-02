namespace ColorBlockJam.UI.Windows
{
    public interface IWindowPresenter
    {
        void Bind(IWindowHost host);

        void Attach(WindowView view);

        void Detach();

        void NotifyShowing();

        void NotifyClosed(bool isCanceled);
    }
}
