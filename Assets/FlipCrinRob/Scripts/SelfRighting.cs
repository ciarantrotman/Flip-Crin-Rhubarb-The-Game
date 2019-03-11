using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace FlipCrinRob.Scripts
{
    public static class SelfRighting
    {
        public static void Torque(Rigidbody rb, Transform transform, float threshold, float force, ForceMode type, bool debug)
        {
            var aUp = Vector3.Angle(Vector3.up, transform.up);

            if (debug)
            {
                DrawRays(transform);
            }
            
            if (aUp < threshold) return;

            var rightingAxis = Vector3.Cross(transform.up, Vector3.up);
            rb.AddTorque(rightingAxis * aUp * force, type);

            if (debug)
            {
                Debug.DrawRay(transform.position, rightingAxis * aUp * force, Color.cyan);
            }
        }

        public static void Upward(Rigidbody rb, Transform transform, float height, float force, ForceMode type, bool debug)
        {
            var localPosition = transform.position;
            var adjustedPos = new Vector3(localPosition.x, localPosition.y + height, localPosition.z);
            
            rb.AddForceAtPosition(Vector3.up * force,adjustedPos, type);

            if (debug)
            {
                Debug.DrawRay(adjustedPos, Vector3.up, Color.yellow);
            }
        }

        private static void DrawRays(Transform t)
        {
            var position = t.position;
            Debug.DrawRay(position, Vector3.up*.5f, Color.green);
            Debug.DrawRay(position, Vector3.forward*.5f, Color.blue);
            Debug.DrawRay(position, Vector3.right*.5f, Color.red);
            Debug.DrawRay(position, t.up, Color.green);
            Debug.DrawRay(position, t.forward, Color.blue);
            Debug.DrawRay(position, t.right, Color.red);
        }
    }
}
