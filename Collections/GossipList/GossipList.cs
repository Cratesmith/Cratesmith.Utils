using System;
using System.Collections;
using System.Collections.Generic;

public class GossipList<T> : IList<T>, IGossipContainer<GossipList<T>>
{
    private List<T>         m_List;

    #region Constructors
    public GossipList()
    {
        m_List = new List<T>();
        m_Reporting = new GossipReporting<GossipList<T>>(this);
    }

    public GossipList(int _capacity)
    {
        m_List = new List<T>(_capacity);
        m_Reporting = new GossipReporting<GossipList<T>>(this);
    }

    public GossipList(ICollection<T> _collection)
    {
        m_List = new List<T>(_collection);
        m_Reporting = new GossipReporting<GossipList<T>>(this);
    }  
    #endregion

    #region implicit operator
    public static implicit operator List<T>(GossipList<T> @this)
    {
        return @this?.m_List;
    }
    #endregion

    #region Reporting
    GossipReporting<GossipList<T>> m_Reporting;
    private IGossipContainer<GossipList<T>> _gossipContainerImplementation;

    public void Pause(object _pauseFor)     => m_Reporting.Pause(_pauseFor);
    public void Resume(object _resumeFor)   => m_Reporting.Resume(_resumeFor);
    public void MarkChanged()               => m_Reporting.MarkChanged();
    public bool IsPaused                    => m_Reporting.IsPaused;
    public long ChangeCount                 => m_Reporting.ChangeCount;
    public event Action<GossipList<T>> OnChanged
    {
        add => m_Reporting.OnChanged += value;
        remove => m_Reporting.OnChanged -= value;
    }
    #endregion

    #region Write Operations
    public void AddRange(ICollection<T> _items)
    {
        var prevCount = m_List.Count;
        m_List.AddRange(_items);
        if (m_List.Count != prevCount)
        {
            MarkChanged();
        }
    }

    private static T[] s_prevList;

    public void Sort(int _index, int _count, IComparer<T> _comparison)
    {
        lock (s_prevList)
        {
            if (s_prevList == null || s_prevList.Length < Count)
            {
                s_prevList = new T[Count];
            }
            CopyTo(s_prevList, 0);
        
            m_List.Sort(_index, _count, _comparison);

            for (int i = 0; i < Count; i++)
            {
                if (Comparer<T>.Default.Compare(s_prevList[i], m_List[i])!=0)
                {
                    MarkChanged();
                    return;
                }
            }
        }
    }

    public void Sort()
    {
        for (int i = 0; i < Count-1; i++)
        {
            if (Comparer<T>.Default.Compare(m_List[i], m_List[i+1]) <= 0) continue;
        
            m_List.Sort();
            MarkChanged();
            return;
        }
    
    }

    public void Sort(IComparer<T> _comparison)
    {
        for (int i = 0; i < Count-1; i++)
        {
            if (_comparison.Compare(m_List[i], m_List[i+1]) <= 0) continue;
        
            m_List.Sort(_comparison);
            MarkChanged();
            return;
        }
    }

    public void Sort(Comparison<T> _comparison)
    {
        for (int i = 0; i < Count-1; i++)
        {
            if (_comparison.Invoke(m_List[i], m_List[i+1]) <= 0) continue;
        
            m_List.Sort(_comparison);
            MarkChanged();
            return;
        }
    }

    public void Add(T item)
    {
        m_List.Add(item);
        MarkChanged();
    }

    public bool Remove(T item)
    {
        if (m_List.Remove(item))
        {
            MarkChanged();
            return true;
        }

        return false;
    }

    public int RemoveAll(Predicate<T> _match)
    {
        var result = m_List.RemoveAll(_match);
        if (result > 0)
        {
            MarkChanged();
        }
        return result;
    }

    public void Clear()
    {
        m_List.Clear();
        MarkChanged();
    }

    public void Insert(int index, T item)
    {
        m_List.Insert(index, item);
        MarkChanged();
    }

    public void RemoveAt(int index)
    {
        m_List.RemoveAt(index);
        MarkChanged();
    }

    public T this[int index]
    {
        get => m_List[index];
        set
        {
            if (Comparer<T>.Default.Compare(m_List[index], value)==0)
            {
                return;
            }

            m_List[index] = value;
            MarkChanged();
        }
    }
    #endregion

    #region Implementation of ICollection<T>
    public bool Contains(T item)
    {
        return m_List.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        m_List.CopyTo(array, arrayIndex);
    }

    public int Count => m_List.Count;

    public bool IsReadOnly => ((IList<T>)m_List).IsReadOnly;
    #endregion

    #region Implementation of IList<T>
    public int IndexOf(T item)
    {
        return m_List.IndexOf(item);
    }
    #endregion

    #region Implementation of IEnumerable
    public List<T>.Enumerator GetEnumerator()
    {
        return m_List.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return m_List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_List.GetEnumerator();
    }
    #endregion
}