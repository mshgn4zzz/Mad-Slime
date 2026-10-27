namespace Game
{
    public class SceneStateMachine : BaseStateMachine<ISceneState>
    {
        public void Start(ISceneState state)
        {
            ArgumentGuard.CheckNotNull(state, nameof(state));

            SwitchTo(state);
        }

        public void Stop(ISceneState state)
        {
            if (ReferenceEquals(CurrentState, state))
            {
                Stop();
            }
        }
    }
}
