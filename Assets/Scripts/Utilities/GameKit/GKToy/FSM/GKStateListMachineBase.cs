using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;

namespace GKToy
{
	public abstract class GKStateListMachineBase<STATE_ID_T>
	{
		List<GKStateMachineStateBase<STATE_ID_T>> _states;
		GKStateMachineStateBase<STATE_ID_T> _defaultState;
		List<GKStateMachineStateBase<STATE_ID_T>> _currentState;
		GKStateMachineStateBase<STATE_ID_T> _lastState;
		List<STATE_ID_T> _currentAddingState;
		List<STATE_ID_T> _currentRemovingState;

		public GKStateListMachineBase()
		{
			_states = new List<GKStateMachineStateBase<STATE_ID_T>>();
			_currentState = new List<GKStateMachineStateBase<STATE_ID_T>>();
			_currentAddingState = new List<STATE_ID_T>();
			_currentRemovingState = new List<STATE_ID_T>();
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

		public List<GKStateMachineStateBase<STATE_ID_T>> GetCurrentState()
		{
			return _currentState;
		}

		public GKStateMachineStateBase<STATE_ID_T> GetLastState()
		{
			return _lastState;
		}

		public void GoToState(STATE_ID_T fromStateId, List<STATE_ID_T> targetStateIds)
		{
			if(targetStateIds != null && targetStateIds.Count > 0)
				_currentAddingState.AddRange(targetStateIds);
			_currentRemovingState.Add(fromStateId);
		}

		public void LeaveState(STATE_ID_T stateId)
		{
			_currentRemovingState.Add(stateId);
		}

		public void Update()
		{
			if (_states.Count == 0)
				return;

			if (_currentState.Count == 0)
			{
				if (_defaultState == null)
					return;
				_currentAddingState.Add(_defaultState.ID);
				_defaultState = null;
			}
			if (0 != _currentRemovingState.Count)
			{
				foreach (var state in _currentRemovingState)
				{
					_RemoveCurrentState(state);
				}
				_currentRemovingState.Clear();
			}
			if (0 != _currentAddingState.Count)
			{
				foreach (var state in _currentAddingState)
				{
					_AddCurrentState(state);
				}
				_currentAddingState.Clear();
			}
			if (0 != _currentState.Count)
			{
				foreach (var state in _currentState)
				{
					state.Update();
				}
			}
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

		void _AddCurrentState(STATE_ID_T stateId)
		{
			GKStateMachineStateBase<STATE_ID_T> state = _GetStateById(stateId);
			if (state != null && !_currentState.Contains(state))
			{
				_currentState.Add(state);
				state.Enter();
			}
		}

		void _RemoveCurrentState(STATE_ID_T stateId)
		{
			GKStateMachineStateBase<STATE_ID_T> state = _GetStateById(stateId);
			if (state != null && _currentState.Contains(state))
			{
				state.Exit();
				_lastState = state;
				_currentState.Remove(state);
			}
		}
	}
}
