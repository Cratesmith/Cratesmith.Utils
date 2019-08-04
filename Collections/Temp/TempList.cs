// Cratesmith 2017
using System;
using System.Collections;
using System.Collections.Generic;
using Cratesmith;
using UnityEngine;

// Temporary list pool for use with unity api
//
// eg: 
// using (var list = TempList<Renderer>.Get()) 
// {
//		transform.GetComponentsInChildren<Renderer>(list);
//		foreach(var r in list) Debug.Log(r.name);
// }
public class TempList<T> : IDisposable, IList<T>
{
    private static readonly PreallocLinkList<TempList<T>> s_lists = new PreallocLinkList<TempList<T>>();
	public readonly List<T> list;

	/// constructor is private. Use satic Get method instead
	private TempList(int minCapacity)
	{
		list = new List<T>(minCapacity);
	}

	// acquire a temporary list
	public static TempList<T> Get(int minCapacity=0)
	{
        lock(s_lists)
        {
	        foreach (var sList in s_lists)
	        {
		        if (sList.list.Capacity < minCapacity) continue;
		        s_lists.Remove(sList);
		        return sList;
	        }


    	    return new TempList<T>(minCapacity*2);
        }
	}

	public static TempList<T> Get<TEnumerator>(TEnumerator enumerator, int minCapacity = 0) where TEnumerator:IEnumerator<T>
	{
		var list = Get(minCapacity);
		list.AddRange(enumerator);
		return list;
	}

	// return a list back to the pool
	public void Dispose()
	{
		list.Clear();
        lock(s_lists)
        {
		    s_lists.Add(this);
        }
	}

	public static implicit operator List<T>(TempList<T> from)
	{
		return from != null ? from.list : null;
	}

	#region IList implementation
	public List<T>.Enumerator GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(T item)
	{
		list.Add(item);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(T item)
	{
		return list.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		return list.Remove(item);
	}

	public int Count
	{
		get { return list.Count; }
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public int IndexOf(T item)
	{
		return list.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		list.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		list.RemoveAt(index);
	}

	public T this[int index]
	{
		get { return list[index]; }
		set { list[index] = value; }
	}
	#endregion

	public void AddRange<TItem>(TItem[] array) where TItem : T
	{
		foreach (var item in array)
		{
			list.Add(item);
		}
	}
	
	public void AddRange<TItem>(List<TItem> list) where TItem : T
	{
		foreach (var item in list)
		{
			this.list.Add(item);
		}
	}

	public void AddRange<TEnumerator>(TEnumerator enumerator, int startAt=0, int count=-1) where TEnumerator:IEnumerator<T>
	{
        while (startAt > 0 && enumerator.MoveNext())
        {
            --startAt;
        }

		while(count!=0 && enumerator.MoveNext())
		{
            list.Add(enumerator.Current);
            --count;
        }
		enumerator.Dispose();
	}

	public T[] ToArray()
	{
		return list.ToArray();
	}

	public void RemoveRange(int index, int count)
	{
		list.RemoveRange(index,count);
	}
}

// Extension methods for using TempLists to handle GetComponentsIn... calls
//
// example usage:
//
// using(var rigidbodies = obj.GetComponentsInChildrenTempList<Rigidbody>())
// foreach (var rigidbody in rigidbodies)
// {
//     Debug.Log(rigidbody.name);
// }
public static class TempListExtensions
{
	public static TempList<T> GetComponentsTempList<T>(this Component @this)
	{
		var tempList = TempList<T>.Get();
		if (@this != null)
		{
			@this.GetComponents<T>(tempList.list);
		}
		return tempList;
	}

	public static TempList<T> GetComponentsInChildrenTempList<T>(this Component @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInChildren<T>(includeInactive, tempList.list);
		}
		return tempList;
	}
 
	public static TempList<T> GetComponentsInParentTempList<T>(this Component @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInParent<T>(includeInactive, tempList.list);
		}
		return tempList;
	}

	public static TempList<T> GetComponentsTempList<T>(this GameObject @this)
	{
		var tempList = TempList<T>.Get();
		if (@this != null)
		{
			@this.GetComponents<T>(tempList.list);
		}
		return tempList;
	}
 
	public static TempList<T> GetComponentsInChildrenTempList<T>(this GameObject @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInChildren<T>(includeInactive, tempList.list);
		}
		return tempList;
	}
 
	public static TempList<T> GetComponentsInParentTempList<T>(this GameObject @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInParent<T>(includeInactive, tempList.list);
		}
		return tempList;
	}
}
