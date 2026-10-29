using System.Threading;
using Cysharp.Threading.Tasks;

namespace MadSlime.Saves.Interfaces
{
    public interface ISaveConfirmation
    {
        UniTask ConfirmSavedAsync(CancellationToken cancellationToken);
    }
}
