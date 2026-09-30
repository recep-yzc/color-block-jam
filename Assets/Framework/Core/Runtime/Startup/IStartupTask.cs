using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.Core.Startup
{
    public interface IStartupTask
    {
        UniTask RunAsync(CancellationToken cancellationToken);
    }
}
