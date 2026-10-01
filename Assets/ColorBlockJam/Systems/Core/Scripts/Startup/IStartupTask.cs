using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Startup
{
    public interface IStartupTask
    {
        UniTask RunAsync(CancellationToken cancellationToken);
    }
}
