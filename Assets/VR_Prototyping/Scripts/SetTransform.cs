using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class SetTransform
    {
        public static void SetPosition(Transform a, Transform b)
        {
            a.position = b.position;
        }
        
        public static void SetRotation(Transform a, Transform b)
        {
            a.rotation = b.rotation;
        }
        
        public static void Follow(Transform a, Transform b)
        {
            SetPosition(a, b);
            SetRotation(a, b);
        }
    } 
}

