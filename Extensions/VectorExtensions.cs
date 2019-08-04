using UnityEngine;

public static class VectorExtensions
{
	public static Vector2 XZ(this Vector3 @this)
	{
		return new Vector2(@this.x, @this.z);
	}

	public static Vector2 XY(this Vector3 @this)
	{
		return new Vector2(@this.x, @this.y);
	}	

    public static Vector3 X_Z(this Vector3 @this)
    {
        return new Vector3(@this.x, 0, @this.z);
    }

	public static Vector3 X_Y(this Vector2 @this)
	{
		return new Vector3(@this.x, 0, @this.y);
	}

	public static Vector3 XY_(this Vector2 @this)
	{
		return new Vector3(@this.x, @this.y, 0f);
	}

	public static Vector4 RGBA(this Color @this)
	{
		return new Vector4(@this.r, @this.g, @this.b, @this.a);
	}

	public static Vector3 RGB(this Color @this)
	{
		return new Vector4(@this.r, @this.g, @this.b);
	}

	public static float ToAngle(this Vector2 @this)
	{
		return 90-Mathf.Atan2(@this.y, @this.x) * Mathf.Rad2Deg;
	}

	public static float FlatDistSq(this Vector3 @from, Vector3 to)
	{
		var diff = (to-@from);
		diff.y = 0;
		return diff.sqrMagnitude;
	}
	
	public static float FlatDist(this Vector3 @this, Vector3 to)
	{
		return Mathf.Sqrt(FlatDistSq(@this, to));
	}

	public static DiffDirDist FlatDiffDistDir(this Vector3 @from, Vector3 to)
	{
		var output = new DiffDirDist();
		output.difference = (to - @from);
		output.difference.y = 0f;

		if (output.difference.sqrMagnitude <= 0f)
		{
			output.distance = 0f;
			output.direction = Vector3.forward;
		}
		else
		{
			output.distance = output.difference.magnitude;
			output.direction = output.difference / output.distance;
		}		
		return output;
	}

	public static DiffDirDist DiffDistDir(this Vector3 @from, Vector3 to)
	{
		var output = new DiffDirDist();
		output.difference = to - @from;
		if (output.difference.sqrMagnitude <= 0f)
		{
			output.distance = 0f;
			output.direction = Vector3.forward;
		}
		else
		{
			output.distance = output.difference.magnitude;
			output.direction = output.difference / output.distance;
		}		
		return output;
	}

	public struct DiffDirDist
	{
		public Vector3 difference;
		public float distance;
		public Vector3 direction;
	}
}