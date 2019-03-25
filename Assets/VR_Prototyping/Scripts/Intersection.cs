using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class Intersection
    {
        public static Vector3 Line(Vector3 point1, Vector3 vector1, Vector3 point2, Vector3 vector2, float tolerance)
        {
            const float epsilon = 0.0001f;
            var v3 = point2 - point1;
            var x1 = Vector3.Cross(vector1, vector2);
            var x2 = Vector3.Cross(v3, vector2);
 
            var planarFactor = Vector3.Dot(v3, x1);

            if (Mathf.Abs(planarFactor) < tolerance && x1.sqrMagnitude > tolerance)
            {
                var s = Vector3.Dot(x2, x1) / x1.sqrMagnitude;
                return point1 + vector1 * s;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}
