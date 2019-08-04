namespace Cratesmith
{
    public interface IState
    {
        void OnEnter();
        void OnExit();       
    }

    public interface IState<TState, TStateMachine> : IState
        where TStateMachine : class, IStateMachine<TState,TStateMachine>
        where TState : class, IState<TState, TStateMachine>
    {
        TStateMachine stateMachine { get; }

        void Init(TStateMachine stateMachine);
        void OnInit();
    }
}