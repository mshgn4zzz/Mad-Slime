using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interfaces
{
    public interface ISceneFsmState
    {
         UniTask PrepareAsync(CancellationToken cancellationToken);
        void Enter();
        void Exit();
    }
}