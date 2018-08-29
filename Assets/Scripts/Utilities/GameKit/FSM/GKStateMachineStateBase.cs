namespace GKStateMachine
{
    public abstract class GKStateMachineStateBase<STATE_ID_T>
    {
        public STATE_ID_T ID { get; private set; }

        public GKStateMachineStateBase(STATE_ID_T id)
        {
            ID = id;
        }

        public abstract void Enter();

        public abstract void Exit();

        public abstract STATE_ID_T Update();
    }
}
