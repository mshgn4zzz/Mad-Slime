using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game
{
    public interface ISessionStateMachine
    {
        UniTask PrepareAsync(CancellationToken cancellationToken);

        void Start();

        void Stop();
    }
}
