using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class Hover
    {
        public static void HoverVector(Rigidbody rb, Transform transform, float height, float force, ForceMode type)
        {
            var forward = transform.forward;
            
            var ray = new Ray (transform.position, forward);
            RaycastHit hit;
            Debug.DrawRay(transform.position, forward * height, Color.cyan);
            
            if (!Physics.Raycast(ray, out hit, height)) return;
            
            var proportionalHeight = (height - hit.distance) / height;
            var appliedHoverForce = -forward * proportionalHeight * force;
            
            rb.AddForce(appliedHoverForce, type);
            
            Debug.DrawRay(transform.position, -forward * proportionalHeight * force, Color.blue);
        }
    }
}

