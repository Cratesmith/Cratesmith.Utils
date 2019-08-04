namespace Cratesmith.Utils
{
    /// <summary>
    /// Interface for a basic state machine. States must be class instances based on IState unique to this state machine
    /// </summary>
    public interface IStateMachine<TState>
        where TState : class, IState        
    {
        TState currentState { get; }
        void SetState(TState newState);
    }

    /// <summary>
    /// Interface for a basic state machine. States must be class instances based on IState unique to this state machine
    /// </summary>
    public interface IStateMachine<TState, TStateMachine> : IStateMachine<TState>
        where TState : class, IState<TState, TStateMachine>
        where TStateMachine : class, IStateMachine<TState, TStateMachine>
    {        
    }
}