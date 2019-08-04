using System;

public interface IGossipContainer<out TSelf> : IGossipContainer
{
    /// <summary>
    /// Called whenever the container has been changed
    /// All changes made to the container while the container is paused are considered a single change that happens when it is unpaused
    /// </summary>
    event Action<TSelf> OnChanged;
}

public interface IGossipContainer
{
    /// <summary>
    /// Pause reporting.
    /// All changes made to the container while the container is paused are considered a single change that happens when it is unpaused
    /// </summary>
    /// <param name="_pauseFor">An object to mark as having paused this container</param>
    void Pause(object _pauseFor);

    /// <summary>
    /// Remove an object from pausing this container. 
    /// All changes made to the container while the container is paused are considered a single change that happens when it is unpaused
    /// </summary>
    /// <param name="_resumeFor"></param>
    void Resume(object _resumeFor);

    /// <summary>
    /// Manually mark the container as changed. 
    /// </summary>
    void MarkChanged();

    /// <summary>
    /// Is the container paused?
    /// This indicates that one or more objects have paused it
    /// </summary>
    bool IsPaused       { get; }

    /// <summary>
    /// The number of times the container has been modified
    /// All changes made to the container while the container is paused are considered a single change that happens when it is unpaused
    /// </summary>
    long ChangeCount    { get; }
}