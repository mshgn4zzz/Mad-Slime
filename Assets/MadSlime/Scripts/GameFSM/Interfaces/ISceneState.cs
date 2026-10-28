using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game
{
    public interface ISceneState : IState
    {
        SceneId SceneId { get; }

        UniTask PrepareAsync(CancellationToken cancellationToken);
    }
}
