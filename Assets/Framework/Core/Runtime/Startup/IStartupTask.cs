using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.Core.Startup
{
    /// <summary>
    /// Work that must finish on the splash screen before the main scene opens,
    /// for example loading save data or warming up services.
    /// </summary>
    public interface IStartupTask
    {
        UniTask RunAsync(CancellationToken cancellationToken);
    }
}
