using System;
namespace UI.Spawning
{
    public interface IShowable
    {
        event Action Closed;

        void Show();

        void Hide();
    }
}
