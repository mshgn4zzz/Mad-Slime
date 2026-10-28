namespace UI.Menu
{
    public class MenuSessionData
    {
        private bool _isDailyShown;

        public bool TryMarkDailyShown()
        {
            if (_isDailyShown)
            {
                return false;
            }

            _isDailyShown = true;
            return true;
        }
    }
}
