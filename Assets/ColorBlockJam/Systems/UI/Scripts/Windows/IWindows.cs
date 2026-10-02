using System;

namespace ColorBlockJam.UI.Windows
{
    public interface IWindows
    {
        event Action BackPressedWithoutWindow;

        bool HasOpenWindow { get; }

        TPresenter Get<TPresenter>() where TPresenter : class, IWindowPresenter;
    }
}
