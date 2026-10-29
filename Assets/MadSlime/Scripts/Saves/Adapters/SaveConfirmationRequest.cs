using Cysharp.Threading.Tasks;

namespace MadSlime.Saves.Adapters
{
    public class SaveConfirmationRequest
    {
        public SaveConfirmationRequest(int id, string json)
        {
            Id = id;
            Json = json;
            Completion = new UniTaskCompletionSource();
        }

        public int Id { get; }
        public string Json { get; }
        public UniTaskCompletionSource Completion { get; }
    }
}
