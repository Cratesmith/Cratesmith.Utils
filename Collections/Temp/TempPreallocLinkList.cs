//#define TEMPPREALLOCLINKLIST_LOGGING
using System;
using System.Collections;
using System.Collections.Generic;
using Cratesmith;
using UnityEngine;

public class TempPreallocLinkList<T> : IDisposable, ICollection<T>
{	
    private static readonly Queue<TempPreallocLinkList<T>> s_lists = new Queue<TempPreallocLinkList<T>>();
    public readonly PreallocLinkList<T> list = new PreallocLinkList<T>();
	static int s_count = 0;
	private int m_id = 0;

    /// constructor is private. Use satic Get method instead
    private TempPreallocLinkList()
    {
	    m_id = s_count;
	    ++s_count;
#if TEMPPREALLOCLINKLIST_LOGGING
		Debug.LogFormat("TempPreallocLinkList<{0}>: creating id:{1}", typeof(T).Name, s_count);
#endif
    }

	~TempPreallocLinkList()
	{
		if (ApplicationState.isQuitting) return;
		Debug.LogWarningFormat("TempPreallocLinkList<{0}>: id:{1} was destroyed, not returned!", typeof(T).Name, m_id);
	}

    // acquire a temporary list
    public static TempPreallocLinkList<T> Get()
    {
        lock (s_lists)
        {
	        if (s_lists.Count == 0)
	        {
		        return new TempPreallocLinkList<T>();
	        }

	        var instance = s_lists.Dequeue();
#if TEMPPREALLOCLINKLIST_LOGGING
	        Debug.LogFormat("TempPreallocLinkList<{0}>: leasing id:{1}", typeof(T).Name, instance.m_id);
#endif
	        return instance;
        }
    }

	public T First()
	{
		foreach (var t in list)
		{
			return t;
		}
		return default(T);
	}

	public int Count {get { return list.Count; }}
	public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

	public void Add(T t)
	{
		list.Add(t);
	}

	public void AddSorted(T item, Comparison<T> comparison)
	{
		list.AddSorted(item, comparison);
	}
	

	public void Clear()
	{
		list.Clear();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(T t)
	{
		return list.Remove(t);
	}

	void ICollection<T>.Add(T item)
	{
		list.Add(item);
	}

	public bool Contains(T t)
	{
		return list.Contains(t);
	}

    // return a list back to the pool
    public void Dispose()
    {
        list.Clear();
        lock (s_lists)
        {
#if TEMPPREALLOCLINKLIST_LOGGING
	        Debug.LogFormat("TempPreallocLinkList<{0}>: returning id:{1}", typeof(T).Name, m_id);
#endif
            s_lists.Enqueue(this);
        }
    }

    public static implicit operator PreallocLinkList<T>(TempPreallocLinkList<T> from)
    {
        return from != null ? from.list : null;
    }

	public IEnumerator<T> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}
}
