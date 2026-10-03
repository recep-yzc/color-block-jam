using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public interface ILevelIntro
    {
        bool IsPending { get; }

        UniTask PresentAsync(CancellationToken cancellationToken);
    }
}
