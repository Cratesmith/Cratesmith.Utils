using System;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsExtensions
{
    public static TempList<Vector3> GetCornersTemplist(this Bounds @this)
    {
        var tempList = TempList<Vector3>.Get();
        @this.GetCorners(tempList);
        return tempList;
    }

    public static void GetCorners(this Bounds @this, List<Vector3> corners)
    {
        corners.Clear();        
	    corners.Add(new Vector3(@this.extents.x, @this.extents.y, @this.extents.z) + @this.center); 
	    corners.Add(new Vector3(@this.extents.x, @this.extents.y, -@this.extents.z) + @this.center);
	    corners.Add(new Vector3(@this.extents.x, -@this.extents.y, @this.extents.z) + @this.center);
	    corners.Add(new Vector3(@this.extents.x, -@this.extents.y, -@this.extents.z) + @this.center);
	    corners.Add(new Vector3(-@this.extents.x, @this.extents.y, @this.extents.z) + @this.center);
	    corners.Add(new Vector3(-@this.extents.x, @this.extents.y, -@this.extents.z) + @this.center);
	    corners.Add(new Vector3(-@this.extents.x, -@this.extents.y, @this.extents.z) + @this.center);
	    corners.Add(new Vector3(-@this.extents.x, -@this.extents.y, -@this.extents.z) + @this.center);
    }

	public static Rect GetScreenRect(this Bounds @this, Camera camera, Transform relativeTo=null, System.Func<Vector3, Vector3> modifyWorldPositionFunc=null)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}		

		using (var list = TempList<Vector3>.Get())
		{
			@this.GetCorners(list);

			if (relativeTo)
			{
				var m = relativeTo.localToWorldMatrix;
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = m.MultiplyPoint(list[i]);
				}
			}

			if (modifyWorldPositionFunc != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = modifyWorldPositionFunc(list[i]);
				}				
			}

			var r = new Rect(camera.WorldToScreenPoint(list[0]), Vector2.zero);
			for (int i = 1; i < list.Count; i++)
			{
				var point = camera.WorldToScreenPoint(list[i]);
				r.xMin = Mathf.Min(r.xMin, point.x);
				r.yMin = Mathf.Min(r.yMin, point.y);
				r.xMax = Mathf.Max(r.xMax, point.x);
				r.yMax = Mathf.Max(r.yMax, point.y);
			}
			return r;
		}
	}

	public static void DrawGizmo(this Bounds @this, Transform transform=null)
	{
		var pop = Gizmos.matrix;
		if (transform!=null)
		{
			Gizmos.matrix *= transform.localToWorldMatrix;			
		}
		Gizmos.DrawWireCube(@this.center, @this.extents*2);
		Gizmos.matrix = pop;
	}
}