using TMPro;
using UnityEngine;
using Valve.VR;

namespace FlipCrinRob.Scripts
{
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms controller;
        
        [SerializeField] private HandleController lHandle;
        [SerializeField] private HandleController cHandle;
        [SerializeField] private HandleController rHandle;

        [SerializeField] private float dual;
        [SerializeField] private float mono;

        [SerializeField] private Rigidbody rb;

        [SerializeField] private TextMeshPro speedText;

        [SerializeField] private Transform l;
        [SerializeField] private Transform r;
        [SerializeField] private Transform v;

        private bool lActive;
        private bool cActive;
        private bool rActive;

        private const float R = 3f;

        private const float HoverHeight = .2f;
        private const float HoverForce = 50f;

        private void Start()
        {
            lHandle.ClipThreshold = dual;
            cHandle.ClipThreshold = mono;
            rHandle.ClipThreshold = dual;
        }

        private void Update()
        {
            lActive = lHandle.Active;
            cActive = cHandle.Active;
            rActive = rHandle.Active;
        }

        private void FixedUpdate()
        {
            Hover();
            
            if (lActive && rActive)
            {
                DualMovement();
            }
            else if (cActive)
            {
                MonoMovement();
            }
        }

        private void Hover()
        {
            Ray ray = new Ray (transform.position, -transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, HoverHeight))
            {
                float proportionalHeight = (HoverHeight - hit.distance) / HoverHeight;
                Vector3 appliedHoverForce = Vector3.up * proportionalHeight * HoverForce;
                rb.AddForce(appliedHoverForce, ForceMode.Acceleration);
            }
        }

        private void DualMovement()
        {
            l.localRotation = controller.LeftControllerTransform().localRotation;
            r.localRotation = controller.RightControllerTransform().localRotation;
            var averageRotation = v.localRotation;
            
            averageRotation = Quaternion.Lerp(averageRotation, 
                Quaternion.Lerp(
                    controller.LeftControllerTransform().localRotation, 
                    controller.RightControllerTransform().localRotation, 
                    .5f), 
                .1f);
            
            v.localRotation = averageRotation;

            rb.AddForce(controller.LeftForwardvector() * lHandle.M);
            rb.AddForce(controller.RightForwardvector() * rHandle.M);               
            rb.AddTorque(transform.up * averageRotation.y * R);
                
            speedText.SetText(lHandle.M +  " | " + rHandle.M);
        }

        private void MonoMovement()
        {
            rb.AddForce(transform.TransformVector(transform.forward) * cHandle.M);
        }
    }
}
