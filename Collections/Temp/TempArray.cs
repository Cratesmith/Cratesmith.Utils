//#define TEMPARRAY_LOGGING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempArray<T> : IDisposable, IEnumerable<T> where T : new()
{
    private static readonly Dictionary<int, Queue<TempArray<T>>> s_queueTable = new Dictionary<int, Queue<TempArray<T>>>();
    public T[] array;
    static int s_count = 0;
    private readonly int m_id = 0;
    private readonly Queue<TempArray<T>> m_queue;

    /// constructor is private. Use satic Get method instead
    private TempArray(int capacity, Queue<TempArray<T>> queue)
    {
        array = new T[capacity];
        m_id = s_count;
        m_queue = queue;
        ++s_count;
#if TEMPARRAY_LOGGING
		Debug.LogFormat("TempArray<{0}[{1}]>: creating id:{2}", typeof(T).Name, capacity, s_count);
#endif
    }

    ~TempArray()
    {
        if (ApplicationState.isQuittingOrNotPlaying) return;
        Debug.LogErrorFormat("TempArray<{0}[{1}]>: id:{2} was destroyed, not returned!", typeof(T).Name, array.Length, m_id);
    }
    
    // acquire a temporary instance
    public static TempArray<T> Get(int capacity)
    {
        lock(s_queueTable)
        {
            Queue<TempArray<T>> list;
            if (!s_queueTable.TryGetValue(capacity, out list))
            {
                list = s_queueTable[capacity] = new Queue<TempArray<T>>();
            }

            if (list.Count == 0)
            {
                return new TempArray<T>(capacity, list);
            }
            var instance = list.Dequeue();
#if TEMPARRAY_LOGGING
	        Debug.LogFormat("TempArray<{0}[{1}]>: leasing id:{2}", typeof(T).Name, capacity, instance.m_id);
#endif
            return instance;
        }
    }

    public static implicit operator T[](TempArray<T> from)
    {
        return from != null ? from.array : null;
    }

    // return a instance back to the pool
    public void Dispose()
    {
        var disposable = array as IDisposable;
        if (disposable!=null)
        {
#if TEMPARRAY_LOGGING
	        Debug.LogFormat("TempArray<{0}[{1}]>: returning id:{1}", capacity, typeof(T).Name, m_id);
#endif
            disposable.Dispose();
        }

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = default(T);
        }

        m_queue.Enqueue(this);
    }

    public T this[int index]
    {
        get { return array[index]; }
        set { array[index] = value; }
    }

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

    public void CopyTo(T[] array, int arrayIndex)
    {
        this.array.CopyTo(array, arrayIndex);
    }

    public int Length
    {
        get { return array.Length; }
    }

	public struct Enumerator : IEnumerator<T>
	{
		private TempArray<T> list;
		private int index;

		public Enumerator(TempArray<T> _list)
		{
			list = _list;
			index = -1;
		}

		public bool MoveNext()
		{
			++index;
			return index < list.array.Length;
		}

		public void Reset()
		{
			index = -1;
		}

		public T Current
		{
			get { return list.array[index]; }
		}
		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}
	}
}