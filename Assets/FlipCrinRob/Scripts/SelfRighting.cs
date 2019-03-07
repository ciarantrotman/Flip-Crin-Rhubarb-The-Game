using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class SelfRighting
    {
        public static void Right(Rigidbody rb, float rotation, Transform transform, Vector3 direction, float threshold, float force)
        {
            var aUp = Vector3.Angle(Vector3.up, transform.up);
            DrawRays(transform);
            
            if (aUp < threshold) return;
            
            switch (rotation < 0)
            {
                case true:
                    rb.AddTorque(direction * force);
                    break;
                case false:
                    rb.AddTorque(-direction * force);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawRays(Transform t)
        {
            var position = t.position;
            Debug.DrawRay(position, Vector3.up, Color.green);
            Debug.DrawRay(position, Vector3.forward, Color.blue);
            Debug.DrawRay(position, Vector3.right, Color.red);
            Debug.DrawRay(position, t.up, Color.green);
            Debug.DrawRay(position, t.forward, Color.blue);
            Debug.DrawRay(position, t.right, Color.red);
        }
    }
}
