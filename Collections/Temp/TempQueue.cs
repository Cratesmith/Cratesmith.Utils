//#define TEMPQUEUE_LOGGING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempQueue<T> : IDisposable, IReadOnlyCollection<T>
{
	private static readonly Queue<TempQueue<T>> s_queues = new Queue<TempQueue<T>>();
	public readonly Queue<T> queue = new Queue<T>();
	static int s_count = 0;
	private int m_id = 0;
#if TEMPQUEUE_LOGGING
	private static StackTrace m_callstack;
#endif


	/// constructor is private. Use satic Get method instead
	private TempQueue()
	{
		m_id = s_count;
		++s_count;
#if TEMPQUEUE_LOGGING
		Debug.LogFormat("TempQueue<{0}>: creating id:{1}", typeof(T).Name, s_count);
#endif
	}
	
	~TempQueue()
	{
		if (ApplicationState.isQuitting) return;
#if !TEMPQUEUE_LOGGING
		Debug.LogWarningFormat("TempQueue<{0}>: id:{1} was destroyed, not returned!", typeof(T).Name, m_id);
#else
		Debug.LogWarningFormat("TempQueue<{0}>: id:{1} was destroyed, not returned! Callstack:\n{2}", typeof(T).Name, m_id, m_callstack);
#endif

	}

	public Queue<T>.Enumerator GetEnumerator()
	{
		return queue.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	// acquire a temporary queue
	public static TempQueue<T> Get()
	{
		lock (s_queues)
		{
			if (s_queues.Count == 0)
			{
				return new TempQueue<T>();
			}
			var instance = s_queues.Dequeue();
#if TEMPQUEUE_LOGGING
	        Debug.LogFormat("TempQueue<{0}>: leasing id:{1}", typeof(T).Name, instance.m_id);
			m_callstack = new StackTrace();
#endif
			return instance;
		}
	}

	// return a queue back to the pool
	public void Dispose()
	{
		queue.Clear();
		lock (s_queues)
		{
#if TEMPQUEUE_LOGGING
	        Debug.LogFormat("TempQueue<{0}>: returning id:{1}", typeof(T).Name, m_id);
#endif
			s_queues.Enqueue(this);
		}
	}

	public static implicit operator Queue<T>(TempQueue<T> from)
	{
		return from != null ? from.queue : null;
	}

	public int Count
	{
		get { return queue.Count; }
	}

	public void Enqueue(T item)
	{
		queue.Enqueue(item);
	}

	public T Dequeue()
	{
		return queue.Dequeue();
	}
}