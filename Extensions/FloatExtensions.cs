using UnityEngine;

namespace Cratesmith
{
    public static class FloatExtensions
    {
        public static Vector2 AngleToVector2(this float @this)
        {
            @this = (@this+90f)*Mathf.Deg2Rad;
            return new Vector2(-Mathf.Cos(@this), Mathf.Sin(@this));
        }

        public static Vector3 AngleToVector3XZ(this float @this)
        {
            @this = (@this+90f)*Mathf.Deg2Rad;
            return new Vector3(-Mathf.Cos(@this), 0, Mathf.Sin(@this));
        }
    }
}