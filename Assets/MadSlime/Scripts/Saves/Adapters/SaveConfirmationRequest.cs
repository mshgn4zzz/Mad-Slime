using Cysharp.Threading.Tasks;
namespace Saves
{
    public class SaveConfirmationRequest
    {
        public int Id { get; }
        public string Json { get; }
        public UniTaskCompletionSource Completion { get; }

        public SaveConfirmationRequest(int id, string json)
        {
            Id = id;
            Json = json;
            Completion = new UniTaskCompletionSource();
        }
    }
}
