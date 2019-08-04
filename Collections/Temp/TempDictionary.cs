//#define TEMPDICTIONARY_LOGGING
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDictionary<TKey,TValue> : IDisposable, IDictionary<TKey,TValue>
{	
	private static readonly Queue<TempDictionary<TKey,TValue>> s_dicts = new Queue<TempDictionary<TKey,TValue>>();
	public readonly Dictionary<TKey,TValue> dict = new Dictionary<TKey,TValue>();
	static int s_count = 0;
	private int m_id = 0;

	/// constructor is private. Use satic Get method instead
	private TempDictionary()
	{
		m_id = s_count;
		++s_count;
#if TEMPDICTIONARY_LOGGING
		Debug.LogFormat("TempDictionary<{0}.{1}>: creating id:{2}", typeof(TKey).Name, typeof(TValue).Name, s_count);
#endif
	}

	~TempDictionary()
	{
		if (ApplicationState.isQuitting) return;
		Debug.LogWarningFormat("TempDictionary<{0},{1}>: id:{2} was destroyed, not returned!", typeof(TKey).Name, typeof(TValue).Name, m_id);
	}

	public Dictionary<TKey,TValue>.Enumerator GetEnumerator()
	{
		return dict.GetEnumerator();
	}
	
	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static TempDictionary<TKey, TValue> Get<TEnumerator>(TEnumerator _enumerator, Func<TValue, TKey> _keyFunc)
		where TEnumerator : IEnumerator<TValue>
	{
		var instance = Get();
		while (_enumerator.MoveNext())
		{
			instance.Add(_keyFunc(_enumerator.Current), _enumerator.Current);
		}
		return instance;
	}

	public static TempDictionary<TKey, TValue> Get<TEnumerator>(TEnumerator _enumerator)
		where TEnumerator : IEnumerator<KeyValuePair<TKey,TValue>>
	{
		var instance = Get();
		while (_enumerator.MoveNext())
		{
			instance.Add(_enumerator.Current.Key, _enumerator.Current.Value);
		}
		return instance;
	}


	// acquire a temporary list
	public static TempDictionary<TKey,TValue> Get()
	{
		lock (s_dicts)
		{
			if (s_dicts.Count == 0)
			{
				return new TempDictionary<TKey,TValue>();
			}

			var instance = s_dicts.Dequeue();
#if TEMPDICTIONARY_LOGGING
	        Debug.LogFormat("TempDictionary<{0},{1}>: leasing id:{2}", typeof(TKey).Name, typeof(TValue).Name, instance.m_id);
#endif
			return instance;
		}
	}

	// return a list back to the pool
	public void Dispose()
	{
		dict.Clear();
		lock (s_dicts)
		{
#if TEMPDICTIONARY_LOGGING
	        Debug.LogFormat("TempDictionary<{0},{1}>: returning id:{2}", typeof(TKey).Name, typeof(TValue).Name, m_id);
#endif
			s_dicts.Enqueue(this);
		}
	}

	public static implicit operator Dictionary<TKey,TValue>(TempDictionary<TKey,TValue> from)
	{
		return from != null ? from.dict : null;
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		((IDictionary<TKey, TValue>) dict).Add(item);
	}

	public void Clear()
	{
		dict.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{		
		return ((IDictionary<TKey,TValue>)dict).Contains(item);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((IDictionary<TKey,TValue>)dict).CopyTo(array,arrayIndex);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return ((IDictionary<TKey,TValue>)dict).Remove(item);
	}

	public int Count => dict.Count;

	public bool IsReadOnly => ((IDictionary<TKey,TValue>)dict).IsReadOnly;

	public void Add(TKey key, TValue value)
	{
		dict.Add(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		return dict.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		return dict.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return dict.TryGetValue(key, out value);
	}

	public TValue this[TKey key]
	{
		get => dict[key];
		set => dict[key] = value;
	}

	public ICollection<TKey> Keys => dict.Keys;

	public ICollection<TValue> Values => dict.Values;
}