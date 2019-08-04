using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cratesmith
{
	public static class GameObjectExtensions
	{
		public static T GetComponentInDirectChild<T>(this GameObject @this) where T : Component
		{
			for (int i = 0; i < @this.transform.childCount; i++)
			{
				var output = @this.transform.GetChild(i).GetComponent<T>();
				if (output) return output;
			}
			return null;
		}

		public static T GetOrAddChild<T>(this GameObject @this, System.Action<T> setup=null) where T:Component
		{
			T output = @this.GetComponentInDirectChild<T>();
			if (output == null)
			{
				var go = new GameObject(typeof(T).Name);
				go.transform.SetParent(@this.transform, false);
				output = go.AddComponent<T>();
				if(setup!=null) setup(output);
			}
			return output;
		}
		
		public static T GetOrAddChild<T>(this GameObject @this, string name, System.Action<T> setup=null) where T:Component
		{
			T output = @this.GetComponentInDirectChild<T>();
			if (output == null)
			{
				var go = new GameObject(name);
				go.transform.SetParent(@this.transform, false);
				output = go.AddComponent<T>();
				if(setup!=null) setup(output);
			}
			return output;
		}

		public static T AddChild<T>(this GameObject @this) where T:Component
		{
			var go = new GameObject(typeof(T).Name);
			go.transform.SetParent(@this.transform, false);
			return typeof(T) == typeof(Transform) ? go.GetComponent<T>() : go.AddComponent<T>();
		}
		
		public static T AddChild<T>(this GameObject @this, string name) where T:Component
		{			
			var go = new GameObject(name);
			go.transform.SetParent(@this.transform, false);
			return typeof(T) == typeof(Transform) ? go.GetComponent<T>() : go.AddComponent<T>();
		}

		public static T GetOrAddComponent<T>(this GameObject @this) where T:Component
		{
			T output = @this.GetComponent<T>();
			if (output == null)
			{
				output = @this.AddComponent<T>();
			}
			return output;
		}

        public static T GetOrAddComponentInChildren<T>(this GameObject @this, bool _includeInactive=false) where T:Component
        {
            T output = @this.GetComponentInChildren<T>(_includeInactive);
            if (output == null)
            {
                output = @this.AddComponent<T>();
            }
            return output;
        }

		public static T GetOrAddComponent<T>(this GameObject @this, System.Action<T> setup) where T:Component
		{
			T output = @this.GetComponent<T>();
			if (output == null)
			{
				output = @this.AddComponent<T>();
				setup(output);
			}
			return output;
		}

		public static TRequired GetOrAddComponent<TRequired, TDefault>(this GameObject @this) where TRequired:Component where TDefault:TRequired
		{
			TRequired output = @this.GetComponent<TRequired>();
			if (output == null)
			{
				output = @this.AddComponent<TDefault>();
			}
			return output;
		}

		public static TRequired GetOrAddComponent<TRequired, TDefault>(this GameObject @this, System.Action<TDefault> setup) where TRequired:Component where TDefault:TRequired
		{
			TRequired output = @this.GetComponent<TRequired>();
			if (output == null)
			{
				var instance = @this.AddComponent<TDefault>();
				setup(instance);
				output = instance;
			}
			return output;
		}
	}
}

