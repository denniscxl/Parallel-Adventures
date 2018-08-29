using System.Collections.Generic;
using UnityEngine;

namespace GKStateMachine
{
    public abstract class GKStateMachineBase<STATE_ID_T>
    {
        List<GKStateMachineStateBase<STATE_ID_T>> _states;
        GKStateMachineStateBase<STATE_ID_T> _defaultState;
        GKStateMachineStateBase<STATE_ID_T> _currentState;
        GKStateMachineStateBase<STATE_ID_T> _lastState;

        public GKStateMachineBase()
        {
            _states = new List<GKStateMachineStateBase<STATE_ID_T>>();
        }

        public void AddState(GKStateMachineStateBase<STATE_ID_T> state, bool asDefault)
        {
            _states.Add(state);
            if (asDefault)
            {
                _defaultState = state;
                _lastState = state;
            }
        }

        public GKStateMachineStateBase<STATE_ID_T> GetCurrentState()
        {
            return _currentState;
        }

        public GKStateMachineStateBase<STATE_ID_T> GetLastState()
        {
            return _lastState;
        }

        public void GoToState(STATE_ID_T targetStateId)
        {
            if (_currentState == null || !targetStateId.Equals(_currentState.ID))
            {
                GKStateMachineStateBase<STATE_ID_T> targetState = _GetStateById(targetStateId);
                if (targetState != null)
                    _GoToState(targetState);
            }
        }

        public void Update()
        {
            if (_states.Count == 0)
                return;

            if (_currentState == null)
            {
                if (_defaultState == null)
                    return;

                _GoToState(_defaultState);
            }

            STATE_ID_T targetStateId = _currentState.Update();

            if (!targetStateId.Equals(_currentState.ID))
                GoToState(targetStateId);
        }

        public GKStateMachineStateBase<STATE_ID_T> _GetStateById(STATE_ID_T id)
        {
            foreach (GKStateMachineStateBase<STATE_ID_T> state in _states)
            {
                if (id.Equals(state.ID))
                    return state;
            }

            return null;
        }

        void _GoToState(GKStateMachineStateBase<STATE_ID_T> targetState)
        {
            if (targetState == null)
                return;

            if (_currentState != null)
                _currentState.Exit();

            if(null != _currentState)
            {
                //Debug.Log(string.Format("Set last state: {0}, Current state: {1}, Target state:{2}",_lastState.ID.ToString(), _currentState.ID.ToString(), targetState.ID.ToString()));
                _lastState = _currentState;
            }
            _currentState = targetState;

            targetState.Enter();
        }
    }

}
