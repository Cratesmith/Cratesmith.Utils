using System;
using System.Collections;
using System.Collections.Generic;

public class GossipDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IGossipContainer<GossipDictionary<TKey,TValue>>
{
    private Dictionary<TKey, TValue> m_Dictionary;

    #region Write operations
    public bool Remove(TKey key)
    {
        if (!m_Dictionary.Remove(key))
        {
            return false;
        }

        MarkChanged();
        return true;
    }

    public TValue this[TKey key]
    {
        get => m_Dictionary[key];
        set
        {
            if (m_Dictionary.TryGetValue(key, out TValue prev)
                && Comparer<TValue>.Default.Compare(prev, value)==0)
            {
                return;
            }

            m_Dictionary[key] = value;
            MarkChanged();
        }
    }

    public void Add(TKey key, TValue item)
    {
        m_Dictionary.Add(key, item);
        MarkChanged();
    }

    public void Clear()
    {
        m_Dictionary.Clear();
        MarkChanged();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        var ic = (ICollection<KeyValuePair<TKey, TValue>>) m_Dictionary;
        ic.Add(item);
        MarkChanged();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        var ic = (ICollection<KeyValuePair<TKey, TValue>>) m_Dictionary;
        if (!ic.Remove(item))
        {
            return false;
        }
        MarkChanged();
        return true;
    }
    #endregion

    #region Operators
    public static implicit operator Dictionary<TKey,TValue>(GossipDictionary<TKey, TValue> @this)
    {
        return @this?.m_Dictionary;
    }
    #endregion

    #region ReadOnly Operations
    public bool ContainsKey(TKey key)
    {
        return m_Dictionary.ContainsKey(key);
    }

    public bool ContainsValue(TValue value)
    {
        return m_Dictionary.ContainsValue(value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return m_Dictionary.TryGetValue(key, out value);
    }

    public int Count => m_Dictionary.Count;

    public Dictionary<TKey, TValue>.KeyCollection Keys => m_Dictionary.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    public Dictionary<TKey, TValue>.ValueCollection Values => m_Dictionary.Values;

    public bool IsReadOnly => ((IDictionary<TKey, TValue>)m_Dictionary).IsReadOnly;
    #endregion

    #region Enumerator
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return m_Dictionary.GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return m_Dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_Dictionary.GetEnumerator();
    }
    #endregion
    
    #region Constructors
    public GossipDictionary()
    {
        m_Dictionary = new Dictionary<TKey, TValue>();
        m_Reporting = new GossipReporting<GossipDictionary<TKey, TValue>>(this);
    }

    public GossipDictionary(IDictionary<TKey, TValue> _dictionary)
    {
        m_Dictionary = new Dictionary<TKey, TValue>(_dictionary);
        m_Reporting = new GossipReporting<GossipDictionary<TKey, TValue>>(this);
    }

    public GossipDictionary(IDictionary<TKey, TValue> _dictionary, IEqualityComparer<TKey> _comparer)
    {
        m_Dictionary = new Dictionary<TKey, TValue>(_dictionary, _comparer);
        m_Reporting = new GossipReporting<GossipDictionary<TKey, TValue>>(this);
    }

    public GossipDictionary(IEqualityComparer<TKey> _comparer)
    {
        m_Dictionary = new Dictionary<TKey, TValue>(_comparer);
        m_Reporting = new GossipReporting<GossipDictionary<TKey, TValue>>(this);
    }

    public GossipDictionary(int _capacity)
    {
        m_Dictionary = new Dictionary<TKey, TValue>(_capacity);
        m_Reporting = new GossipReporting<GossipDictionary<TKey, TValue>>(this);
    }

    public GossipDictionary(int _capacity, IEqualityComparer<TKey> _comparer)
    {
        m_Dictionary = new Dictionary<TKey, TValue>(_capacity, _comparer);
        m_Reporting = new GossipReporting<GossipDictionary<TKey, TValue>>(this);
    }
    #endregion

    #region ICollection 
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        var ic = (ICollection<KeyValuePair<TKey, TValue>>) m_Dictionary;
        return ic.Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var ic = (ICollection<KeyValuePair<TKey, TValue>>) m_Dictionary;
        ic.CopyTo(array, arrayIndex);
    }
    #endregion

    #region Reporting
    GossipReporting<GossipDictionary<TKey,TValue>> m_Reporting;
    private IGossipContainer<GossipDictionary<TKey, TValue>> _gossipContainerImplementation;

    public void Pause(object _pauseFor)     => m_Reporting.Pause(_pauseFor);
    public void Resume(object _resumeFor)   => m_Reporting.Resume(_resumeFor);
    public void MarkChanged()               => m_Reporting.MarkChanged();
    public bool IsPaused                    => m_Reporting.IsPaused;
    public long ChangeCount                 => m_Reporting.ChangeCount;
    public event Action<GossipDictionary<TKey, TValue>> OnChanged
    {
        add => m_Reporting.OnChanged += value;
        remove => m_Reporting.OnChanged -= value;
    }
    #endregion
}