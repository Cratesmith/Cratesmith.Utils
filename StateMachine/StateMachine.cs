namespace Cratesmith.Utils
{
    public class StateMachine<TState, TStateMachine> : IStateMachine<TState, TStateMachine> 
        where TState : class, IState<TState, TStateMachine> 
        where TStateMachine : StateMachine<TState, TStateMachine>
    {
        public TState currentState { get; private set; }
	    public event System.Action<TState, TState> onStateChanged;

        public virtual void SetState(TState newState)
        {
            if (newState == currentState)
            {
                return;
            }

            if (currentState != null)
            {
                currentState.OnExit();
            }

	        var prevState = currentState;
            currentState = newState;

            if (currentState != null)
            {
                if (newState.stateMachine != this)
                {
                    newState.Init(this as TStateMachine);
                }
                currentState.OnEnter();
            }

	        OnStateChanged(currentState,prevState);
	        if (onStateChanged!= null)
	        {
		        onStateChanged(currentState, prevState);
	        }
        }

	    protected virtual void OnStateChanged(TState newState, TState prevState)
	    {
	    }

	    public void Invoke(System.Action<TState> stateAction)
        {
            if (currentState != null && stateAction!=null)
            {
                stateAction.Invoke(currentState);
            }
        }        
    }
}