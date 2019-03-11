using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms controller;
        [SerializeField] private Transform camera;
        
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

        [SerializeField] private Transform thrusters;
        public GameObject HandleVisual;
        
        private bool lActive;
        private bool cActive;
        private bool rActive;

        private const float R = 10f;
        private const float HoverHeight = 1f;
        private const float HoverForce = 7f;
        private const float RightingThreshold = 0f;
        private const float RightingForce = 2f;
        private const float UpwardForce = 1f;
        private const float UpwardHeight = 1f;

        private void Start()
        {
            lHandle.ClipThreshold = dual;
            cHandle.ClipThreshold = mono;
            rHandle.ClipThreshold = dual;
            SetPosition(camera.localPosition, transform.localPosition);
        }

        private void SetPosition(Vector3 a, Vector3 b)
        {
            transform.localPosition = new Vector3(a.x, b.y, a.z + .5f);
        }

        private void Update()
        {
            lActive = lHandle.Active;
            cActive = cHandle.Active;
            rActive = rHandle.Active;
        }

        private void FixedUpdate()
        {
            ConstantForces(ForceMode.Acceleration);
            
            if (lActive && rActive)
            {
                DualMovement();
            }
            else if (cActive)
            {
                MonoMovement();
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
            
            rb.AddForce(NormalisedForwardVector(controller.LeftForwardVector(), .25f) * lHandle.M, ForceMode.Acceleration);
            rb.AddForce(NormalisedForwardVector(controller.RightForwardVector(), .25f) * rHandle.M, ForceMode.Acceleration);               
            rb.AddTorque(transform.up * averageRotation.y * R, ForceMode.Acceleration);
            
            speedText.SetText("{0:2} | {1:2}",lHandle.M, rHandle.M);
        }

        private void MonoMovement()
        {
            rb.AddForce(transform.TransformVector(transform.forward) * cHandle.M);
        }

        private static Vector3 NormalisedForwardVector(Vector3 v, float d)
        {
            return new Vector3(v.x, v.y * d, v.z);
        }

        private void ConstantForces(ForceMode type)
        {
            foreach (Transform x in thrusters)
            {
                Hover.HoverVector(rb, x, HoverHeight, HoverForce, type, controller.debugActive);
            }
            
            SelfRighting.Torque(rb, transform, RightingThreshold, RightingForce, type, controller.debugActive);
            //SelfRighting.Upward(rb, transform, UpwardHeight, UpwardForce, type, debug);
        }
    }
}
