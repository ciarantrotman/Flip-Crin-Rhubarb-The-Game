using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class Set
    {
        public static void Position(Transform a, Transform b)
        {
            a.transform.position = b.transform.position;
        }
        
        public static void Rotation(Transform a, Transform b)
        {
            a.transform.rotation = b.transform.rotation;
        }
        
        public static void Transforms(Transform a, Transform b)
        {
            var A = a.transform;
            var B = b.transform;
            A.position = B.position;
            A.rotation = B.rotation;
        }
    }
}
