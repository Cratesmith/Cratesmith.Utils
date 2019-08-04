using UnityEngine;

namespace Cratesmith
{
	public static class Curves
	{
		public static float Hermite(float p0, float m0, float p1, float m1, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return p0 * (2 * t3 - 3 * t2 + 1) + m0 * (t3 - 2 * t2 + t) + p1 * (-2 * t3 + 3 * t2) + m1 * (t3 - t2);
		}

		public static Vector2 Hermite(Vector2 p0, Vector2 m0, Vector2 p1, Vector2 m1, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return p0 * (2 * t3 - 3 * t2 + 1) + m0 * (t3 - 2 * t2 + t) + p1 * (-2 * t3 + 3 * t2) + m1 * (t3 - t2);
		}

		public static Vector3 Hermite(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return p0 * (2 * t3 - 3 * t2 + 1) + m0 * (t3 - 2 * t2 + t) + p1 * (-2 * t3 + 3 * t2) + m1 * (t3 - t2);
		}

		public static Vector3 HermiteTangent(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float t)
		{
			float t2 = t * t;
			return p0 * (6 * t2 - 6 * t) + m0 * (3 * t2 - 4 * t + 1) + p1 * (-6 * t2 + 6 * t) + m1 * (3 * t2 - 2 * t);
		}

		public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return 0.5f * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
		}
	}
}