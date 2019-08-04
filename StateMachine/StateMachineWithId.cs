using System;
using System.Collections.Generic;

namespace Cratesmith.Utils
{
    // State machine with registered states
    public class StateMachineWithId<TState, TStateMachine, TStateId> : StateMachine<TState, TStateMachine>, IStateMachineWithId<TState, TStateId> 
        where TStateMachine : StateMachineWithId<TState, TStateMachine, TStateId>
        where TState : class, IState<TState, TStateMachine> 
        where TStateId : struct // value types only
    {
        private readonly Dictionary<TStateId, TState> m_idToState = new Dictionary<TStateId, TState>();
        private readonly Dictionary<TState, TStateId> m_stateToId = new Dictionary<TState, TStateId>();

        public TStateId currentStateId { get; private set; }
        public TState defaultState { get; protected set; }

        public bool AddState(TStateId id, TState state)
        {
            if(m_idToState.ContainsKey(id))
            {
                return false;
            }

            m_idToState[id] = state;
            if (state != null)
            {
                m_stateToId[state] = id;                
            }

            if (id.Equals(default(TStateId)))
            {
                defaultState = state;
            }
            else if (state == null)
            {
                throw new ArgumentException("Only the default state can be null");
            }

            return true;
        }

        public bool RemoveState(TStateId id)
        {
            TState state;
            if(m_idToState.TryGetValue(id, out state))
            {
                m_idToState.Remove(id);
                if (state != null)
                {
                    m_stateToId.Remove(state);                    
                }

                if (id.Equals(default(TStateId)))
                {
                    defaultState = null;
                }
                return true;
            }
            return false;
        }

        public bool RemoveState(TState state)
        {
            TStateId id;
            if (m_stateToId.TryGetValue(state, out id))
            {
                m_idToState.Remove(id);
                m_stateToId.Remove(state);

                if (id.Equals(default(TStateId)))
                {
                    defaultState = null;
                }
                return true;
            }
            return false;
        }

        public void SetState(TStateId newStateId)
        {
            TState state = null;
            if(!m_idToState.TryGetValue(newStateId, out state))
            {
                throw new System.ArgumentException(string.Format("No registered StateId {0}", newStateId.ToString()));
            }
            SetState(state);
        }

        public override void SetState(TState newState)
        {
            if(newState!=defaultState && !m_stateToId.ContainsKey(newState))
            {
                throw new System.ArgumentException(string.Format("Cannot call SetState unless state has been registered with a StateId"));
            }

	        if (newState == currentState)
	        {
		        return;
	        }

            base.SetState(newState);	        
        }

	    protected override void OnStateChanged(TState newState, TState prevState)
	    {
		    TStateId id;
		    currentStateId = currentState != null && m_stateToId.TryGetValue(currentState, out id)
			    ? id
			    : default(TStateId);
	    }
    }    
}