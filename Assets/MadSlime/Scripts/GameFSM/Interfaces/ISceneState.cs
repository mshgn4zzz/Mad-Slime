using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game
{
    public interface ISceneState : IState
    {
        UniTask PrepareAsync(CancellationToken cancellationToken);
    }
}
