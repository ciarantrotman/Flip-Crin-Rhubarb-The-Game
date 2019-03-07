using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class SelfRighting
    {
        public static void Right(Rigidbody rb, Transform transform, float threshold, float force, ForceMode type, bool debug)
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
