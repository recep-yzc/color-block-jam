using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public interface ILevelIntro
    {
        UniTask PresentAsync(CancellationToken cancellationToken);
    }
}
