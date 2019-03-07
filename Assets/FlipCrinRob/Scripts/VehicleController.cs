using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        [SerializeField] private List<Transform> thrusters;

        private bool lActive;
        private bool cActive;
        private bool rActive;

        private const float R = 7f;

        private const float HoverHeight = .5f;
        private const float HoverForce = 25f;
        private const float RightingThreshold = 20f;
        private const float RightingForce = 10f;

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
            foreach (var x in thrusters)
            {
                //Hover.HoverVector(rb, x, HoverHeight, HoverForce, ForceMode.Acceleration);
            }

            var t = transform;
            var rotation = t.rotation;
            
            SelfRighting.Right(rb, rotation.x, t, t.right, RightingThreshold, RightingForce);
            SelfRighting.Right(rb, rotation.z, t, t.forward, RightingThreshold, RightingForce);
            
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
            
            rb.AddForce(NormalisedForwardVector(controller.LeftForwardvector(), .25f) * lHandle.M, ForceMode.Acceleration);
            rb.AddForce(NormalisedForwardVector(controller.RightForwardvector(), .25f) * rHandle.M, ForceMode.Acceleration);               
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
    }
}
