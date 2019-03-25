using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class InverseLerp
    {
        public static float Lerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }
    }
}
