using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.UI.Windows
{
    public interface IWindowHost
    {
        bool IsOpen(IWindowPresenter presenter);

        UniTask ShowAsync(IWindowPresenter presenter, CancellationToken cancellationToken);

        void Close(IWindowPresenter presenter);
    }
}
