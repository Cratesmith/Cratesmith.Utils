namespace Cratesmith.Utils
{
    public abstract class State<TState, TStateMachine> : IState<TState, TStateMachine>
        where TState : State<TState, TStateMachine> 
        where TStateMachine : StateMachine<TState, TStateMachine>
    {
        public virtual void OnInit()
        {
        }
        
        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public void Init(TStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            OnInit();
        }

        public TStateMachine stateMachine { get; private set; }
    }
}