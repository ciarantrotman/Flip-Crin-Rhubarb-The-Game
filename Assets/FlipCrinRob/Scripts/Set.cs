using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class Set
    {
        public static void Position(Transform a, Transform b)
        {
            if (a == null || b == null) return;
            a.transform.position = b.transform.position;
        }
        public static void Rotation(Transform a, Transform b)
        {
            if (a == null || b == null) return;
            a.transform.rotation = b.transform.rotation;
        }
        
        public static void SplitPosition(Transform xz, Transform y, Transform c)
        {
            if (xz == null || y == null || c == null) return;
            var position = xz.position;
            c.transform.position = new Vector3(position.x, y.position.y, position.z);
        }
        
        public static void LerpPosition(Transform a, Transform b, float l)
        {
            if (a == null || b == null) return;
            a.position = Vector3.Lerp(a.position, b.position, l);
        }
        
        public static void Transforms(Transform a, Transform b)
        {
            if (a == null || b == null) return;
            Position(a, b);
            Rotation(a, b);
        }

        public static float Midpoint(Transform a, Transform b)
        {
            if (a == null || b == null) return 0f;
            return Vector3.Distance(a.position, b.position) / 2;
        }

        public static void AddForcePosition(Rigidbody rb, Transform a, Transform b, bool debug)
        {
            if (a == null || b == null) return;
            
            var aPos = a.position;
            var bPos = b.position;
            var x = bPos - aPos;
            var d = Vector3.Distance(aPos, bPos) * 10f;
            var p = Mathf.Pow(d, 2);
            var y = 1 / d;
            
            if (debug)
            {
                Debug.DrawRay(aPos, -x * y, Color.cyan);   
                Debug.DrawRay(aPos, x * d, Color.red);
            }
            
            rb.AddForce(x * d);
            
            if (!(d < .5f)) return;
            rb.AddForce(-x * (1 / y));
        }

        public static void RigidBody(Rigidbody rb, float drag, bool stop, bool gravity)
        {
            rb.drag = drag;
            rb.angularDrag = drag;
            rb.velocity = stop? Vector3.zero : rb.velocity;
            rb.useGravity = gravity;
        }
    }
}
