using UnityEngine;

namespace Candy.Unity
{
    public static class Vector3Extensions
    {
        public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
    }
}
