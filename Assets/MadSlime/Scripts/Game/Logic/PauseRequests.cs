using MadSlime.Validation;

namespace MadSlime.Game.Logic
{
    public class PauseRequests
    {
        private readonly Pauser _pauser;
        private int _count;

        public PauseRequests(Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));
            _pauser = pauser;
        }

        public int Count => _count;

        public void Acquire()
        {
            _count++;
            _pauser.IncreasePauseRequests();
        }

        public void Release()
        {
            if (_count == 0)
            {
                return;
            }

            _count--;
            _pauser.DecreasePauseRequests();
        }

        public void ReleaseAll()
        {
            while (_count > 0)
            {
                Release();
            }
        }
    }
}
