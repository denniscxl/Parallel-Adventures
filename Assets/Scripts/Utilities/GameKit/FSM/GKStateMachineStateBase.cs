using UnityEngine;

namespace GKStateMachine
{
	public abstract class GKStateMachineStateBase<STATE_ID_T>
	{
		[SerializeField]
		private STATE_ID_T stateId;
		public STATE_ID_T ID
		{
			get { return stateId; }
			private set { stateId = value; }
		}

        public GKStateMachineStateBase(STATE_ID_T id)
        {
            ID = id;
        }

        public abstract void Enter();

        public abstract void Exit();

        public abstract STATE_ID_T Update();
    }
}
