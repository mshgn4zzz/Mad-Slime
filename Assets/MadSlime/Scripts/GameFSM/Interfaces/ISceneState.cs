using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.StateMachine.Interfaces;

namespace MadSlime.GameFSM.Interfaces
{
    public interface ISceneState : IState
    {
        UniTask PrepareAsync(CancellationToken cancellationToken);
    }
}
