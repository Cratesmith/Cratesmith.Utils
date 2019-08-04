namespace Cratesmith.Utils
{
    /// <summary>
    /// Interface for a state machine with an Id type for states. States must be class instances based on IState unique to this state machine
    /// </summary>
    public interface IStateMachineWithId<TStateId>
    {
        TStateId    currentStateId { get; }
        void SetState(TStateId newStateId);       
    }

    /// <summary>
    /// Interface for a state machine with an Id type for states. States must be class instances based on IState unique to this state machine
    /// </summary>
    public interface IStateMachineWithId<TState, TStateId> 
        : IStateMachine<TState>, IStateMachineWithId<TStateId>
        where TState : class, IState
        where TStateId : struct
    {
        TState      defaultState { get; }
    }
}