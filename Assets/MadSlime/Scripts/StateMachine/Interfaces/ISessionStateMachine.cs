using System.Threading;
using Cysharp.Threading.Tasks;

namespace MadSlime.StateMachine.Interfaces
{
    public interface ISessionStateMachine
    {
        void Start();

        UniTask PrepareAsync(CancellationToken cancellationToken);

        void Stop();
    }
}
