using System;
using System.Collections;
using System.Collections.Generic;

namespace Cratesmith.Utils
{
    public class GossipHashSet<T> : ICollection<T>, IGossipContainer<GossipHashSet<T>>
    {
        private HashSet<T> m_HashSet;   

        #region Constructors
        public GossipHashSet()
        {
            m_HashSet = new HashSet<T>();
            m_Reporting = new GossipReporting<GossipHashSet<T>>(this);
        }
    
        public GossipHashSet(IEnumerable<T> _collection)
        {
            m_HashSet = new HashSet<T>(_collection);
            m_Reporting = new GossipReporting<GossipHashSet<T>>(this);
        }
    
        public GossipHashSet(IEnumerable<T> _collection, IEqualityComparer<T> _comparer)
        {
            m_HashSet = new HashSet<T>(_collection, _comparer);
            m_Reporting = new GossipReporting<GossipHashSet<T>>(this);
        }
        
        public GossipHashSet(IEqualityComparer<T> _comparer)
        {
            m_HashSet = new HashSet<T>(_comparer);
            m_Reporting = new GossipReporting<GossipHashSet<T>>(this);
        }
        #endregion

        #region implicit operator
        public static implicit operator HashSet<T>(GossipHashSet<T> @this)
        {
            return @this?.m_HashSet;
        }
        #endregion

        #region Write operations
        public bool Remove(T item)
        {
            if (!m_HashSet.Remove(item)) return false;
            MarkChanged();
            return true;
        }
    
        public bool Add(T item)
        {
            if (!m_HashSet.Add(item)) return false;
            MarkChanged();
            return true;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            m_HashSet.ExceptWith(other);        
            MarkChanged();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            m_HashSet.IntersectWith(other);
            MarkChanged();
        }

        public int RemoveWhere(Predicate<T> match)
        {
            var result = m_HashSet.RemoveWhere(match);
            if (result > 0)
            {
                MarkChanged();
            }

            return result;
        }
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            m_HashSet.SymmetricExceptWith(other);
            MarkChanged();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            m_HashSet.UnionWith(other);
            MarkChanged();
        }

        public void Clear()
        {
            m_HashSet.Clear();
            MarkChanged();
        }
        #endregion

        #region Hashset Readonly methods
        public bool SetEquals(IEnumerable<T> other)
        {
            return m_HashSet.SetEquals(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return m_HashSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return m_HashSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return m_HashSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return m_HashSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return m_HashSet.Overlaps(other);
        }
        #endregion
    
        #region Implementation of ICollection<T>
        public bool Contains(T item)
        {
            return m_HashSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_HashSet.CopyTo(array, arrayIndex);
        }

        public int Count => m_HashSet.Count;

        public bool IsReadOnly => ((ICollection<T>)m_HashSet).IsReadOnly;

        #endregion

        #region Implementation of IEnumerable
        public HashSet<T>.Enumerator GetEnumerator()
        {
            return m_HashSet.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_HashSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_HashSet.GetEnumerator();
        }
        #endregion

        #region Reporting
        GossipReporting<GossipHashSet<T>> m_Reporting;
        private IGossipContainer<GossipHashSet<T>> _gossipContainerImplementation;

        public void Pause(object _pauseFor)     => m_Reporting.Pause(_pauseFor);
        public void Resume(object _resumeFor)   => m_Reporting.Resume(_resumeFor);
        public void MarkChanged()               => m_Reporting.MarkChanged();
        public bool IsPaused                    => m_Reporting.IsPaused;
        public long ChangeCount                 => m_Reporting.ChangeCount;
        public event Action<GossipHashSet<T>> OnChanged
        {
            add => m_Reporting.OnChanged += value;
            remove => m_Reporting.OnChanged -= value;
        }
        #endregion
    }
}