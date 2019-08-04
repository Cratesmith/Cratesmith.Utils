using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scope for temporarily pausing a IGossipContainer
/// Pauses the container when constructed and resumes it when disposed.
/// Usage: using(new GossipPauseScope(_container)) { ... }
/// </summary>
public struct GossipPauseScope : IDisposable
{
    private const int INIT_COUNT = 10;
    private static Queue<object>    s_PausePool = new Queue<object>();
    private object                  m_Pause;
    private IGossipContainer        m_Target;

    static GossipPauseScope()
    {
        Debug.Log("Initializing GossipPauseScope pool");
        for (int i = 0; i < INIT_COUNT; i++)
        {
            s_PausePool.Enqueue(new object());
        }
    }

    /// <summary>
    /// Create a temporary pause scope for a gossip container
    /// Usage: using(new GossipPauseScope(_container)) { ... }
    /// </summary>
    /// <param name="_target">The container to pause</param>
    public GossipPauseScope(IGossipContainer _target)
    {
        m_Target = _target;
        lock (s_PausePool)
        {
            m_Pause = s_PausePool.Count > 0 
                ? s_PausePool.Dequeue() 
                : new object();

            m_Target?.Pause(m_Pause);            
        }
    }

    /// <summary>
    /// Manually dispose the scope, resuming the container.
    /// </summary>
    public void Dispose()
    {
        m_Target?.Resume(m_Pause);
        if (m_Pause == null) return;
        lock (s_PausePool)
        {
            s_PausePool.Enqueue(m_Pause);
            m_Pause = null;
        }
    }
}