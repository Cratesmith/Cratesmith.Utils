//#define TEMPHASHSET_LOGGING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cratesmith.Utils
{
    public class TempHashSet<T> : IDisposable, ICollection<T>
    {	
        private static readonly Queue<TempHashSet<T>> s_hashSets = new Queue<TempHashSet<T>>();
        public readonly HashSet<T> hashSet = new HashSet<T>();
        static int s_count = 0;
        private int m_id = 0;

        /// constructor is private. Use satic Get method instead
        private TempHashSet()
        {
            m_id = s_count;
            ++s_count;
#if TEMPHASHSET_LOGGING
		Debug.LogFormat("TempHashSet<{0}>: creating id:{1}", typeof(T).Name, s_count);
#endif
        }

        ~TempHashSet()
        {
            if (ApplicationState.isQuitting) return;
            Debug.LogWarningFormat("TempHashSet<{0}>: id:{1} was destroyed, not returned!", typeof(T).Name, m_id);
        }
	
        public static TempHashSet<T> Get<TEnumerator>(TEnumerator source) where TEnumerator:IEnumerator<T>
        {
            var instance = Get();
            while (source.MoveNext())
            {
                instance.Add(source.Current);
            }
            source.Dispose();
            return instance;
        }

        // acquire a temporary list
        public static TempHashSet<T> Get()
        {
            lock (s_hashSets)
            {
                if (s_hashSets.Count == 0)
                {
                    return new TempHashSet<T>();
                }

                var instance = s_hashSets.Dequeue();
#if TEMPHASHSET_LOGGING
	        Debug.LogFormat("TempHashSet<{0}>: leasing id:{1}", typeof(T).Name, instance.m_id);
#endif
                return instance;
            }
        }

        public T First()
        {
            foreach (var t in hashSet)
            {
                return t;
            }
            return default(T);
        }

        public int Count {get { return hashSet.Count; }}
        public bool IsReadOnly => ((ICollection<T>)hashSet).IsReadOnly;

        public bool Add(T t)
        {
            return hashSet.Add(t);
        }

        public void Clear()
        {
            hashSet.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            hashSet.CopyTo(array, arrayIndex);
        }

        public bool Remove(T t)
        {
            return hashSet.Remove(t);
        }

        void ICollection<T>.Add(T item)
        {
            hashSet.Add(item);
        }

        public bool Contains(T t)
        {
            return hashSet.Contains(t);
        }

        // return a list back to the pool
        public void Dispose()
        {
            hashSet.Clear();
            lock (s_hashSets)
            {
#if TEMPHASHSET_LOGGING
	        Debug.LogFormat("TempHashSet<{0}>: returning id:{1}", typeof(T).Name, m_id);
#endif
                s_hashSets.Enqueue(this);
            }
        }

        public static implicit operator HashSet<T>(TempHashSet<T> from)
        {
            return from != null ? from.hashSet : null;
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            return hashSet.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return hashSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return hashSet.GetEnumerator();
        }

        public void UnionWith<TItem>(TItem[] array) where TItem : T
        {
            AddRange(array);
        }

        public void AddRange<TItem>(TItem[] array) where TItem : T
        {
            foreach (var item in array)
            {
                hashSet.Add(item);
            }
        }

        public void UnionWith<TItem>(List<TItem> list) where TItem : T
        {
            AddRange(list);
        }

        public void AddRange<TItem>(List<TItem> list) where TItem : T
        {
            foreach (var item in list)
            {
                hashSet.Add(item);
            }
        }

        public void UnionWith<TEnumerator>(TEnumerator enumerator) where TEnumerator : IEnumerator<T>
        {
            AddRange(enumerator);
        }

        public void AddRange<TEnumerator>(TEnumerator enumerator) where TEnumerator:IEnumerator<T>
        {
            while(enumerator.MoveNext())
            {
                hashSet.Add(enumerator.Current);
            }
            enumerator.Dispose();
        }

        public void ExceptWith<TItem>(List<TItem> list) where TItem:T
        {
            foreach (var item in list)
            {
                hashSet.Remove(item);	
            }
        }

        public void ExceptWith<TEnumerator>(TEnumerator enumerator) where TEnumerator:IEnumerator<T>
        {
            while(enumerator.MoveNext())
            {
                hashSet.Remove(enumerator.Current);	
            }
            enumerator.Dispose();
        }
    }

// Extension methods for using TempHashSets to handle GetComponentsIn... calls
//
// example usage:
//
// using(var rigidbodies = obj.GetComponentsInChildrenTempHashSet<Rigidbody>())
// foreach (var rigidbody in rigidbodies)
// {
//     Debug.Log(rigidbody.name);
// }
    public static class TempHashSetExtensions
    {	
        public static TempHashSet<T> GetComponentsTempHashSet<T>(this Component @this)
        {
            using (var templist = @this.GetComponentsTempHashSet<T>())
            {
                var tempHashSet = TempHashSet<T>.Get();
                tempHashSet.hashSet.UnionWith(templist);
                return tempHashSet;
            }
        }

        public static TempHashSet<T> GetComponentsInChildrenTempHashSet<T>(this Component @this, bool includeInactive=false)
        {
            using (var templist = @this.GetComponentsInChildrenTempList<T>(includeInactive))
            {
                var tempHashSet = TempHashSet<T>.Get();
                tempHashSet.hashSet.UnionWith(templist);
                return tempHashSet;
            }
        }
 
        public static TempHashSet<T> GetComponentsInParentTempHashSet<T>(this Component @this, bool includeInactive=false)
        {
            using (var templist = @this.GetComponentsInParentTempList<T>(includeInactive))
            {
                var tempHashSet = TempHashSet<T>.Get();
                tempHashSet.hashSet.UnionWith(templist);
                return tempHashSet;
            }
        }

        public static TempHashSet<T> GetComponentsTempHashSet<T>(this GameObject @this)
        {
            using (var templist = @this.GetComponentsTempList<T>())
            {
                var tempHashSet = TempHashSet<T>.Get();
                tempHashSet.hashSet.UnionWith(templist);
                return tempHashSet;
            }
        }
 
        public static TempHashSet<T> GetComponentsInChildrenTempHashSet<T>(this GameObject @this, bool includeInactive=false)
        {
            using (var templist = @this.GetComponentsInChildrenTempList<T>(includeInactive))
            {
                var tempHashSet = TempHashSet<T>.Get();
                tempHashSet.hashSet.UnionWith(templist);
                return tempHashSet;
            }
        }
 
        public static TempHashSet<T> GetComponentsInParentTempHashSet<T>(this GameObject @this, bool includeInactive=false)
        {
            using (var templist = @this.GetComponentsInParentTempList<T>(includeInactive))
            {
                var tempHashSet = TempHashSet<T>.Get();
                tempHashSet.hashSet.UnionWith(templist);
                return tempHashSet;
            }
        }
    }
}