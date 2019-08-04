using System;
using System.Collections.Generic;

public class TempInstance<T> : IDisposable where T:new()
{
	private static readonly Queue<TempInstance<T>> s_lists = new Queue<TempInstance<T>>();
	public T value = new T();

	// acquire a temporary instance
	public static TempInstance<T> Get()
	{
		return s_lists.Count > 0
			? s_lists.Dequeue()
			: new TempInstance<T>();
	}

	public static implicit operator T(TempInstance<T> from)
	{
		return from != null ? from.value : default(T);
	}

	// return a instance back to the pool
	public void Dispose()
	{
	    var disposable = value as IDisposable;
	    if (disposable!=null)
	    {
	        disposable.Dispose();
	    }

		s_lists.Enqueue(this);
	}
}