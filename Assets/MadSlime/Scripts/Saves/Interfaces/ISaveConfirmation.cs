using Cysharp.Threading.Tasks;
using System.Threading;
namespace Saves
{
    public interface ISaveConfirmation
    {
        UniTask ConfirmSavedAsync(CancellationToken cancellationToken);
    }
}
