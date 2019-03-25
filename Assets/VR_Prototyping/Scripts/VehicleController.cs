using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public class VehicleController : MonoBehaviour
    {
        [TabGroup("References")][SerializeField] private ControllerTransforms controller;
        [TabGroup("References")][SerializeField] private HandleController lHandle;
        [TabGroup("References")][SerializeField] private HandleController cHandle;
        [TabGroup("References")][SerializeField] private HandleController rHandle;
        [TabGroup("References")][SerializeField] private TextMeshPro speedText;
        [TabGroup("References")][SerializeField] private Transform l;
        [TabGroup("References")][SerializeField] private Transform r;
        [TabGroup("References")][SerializeField] private Transform v;
        [TabGroup("References")][SerializeField] private Transform thrusters;
        
        [TabGroup("Settings")][SerializeField] private float dual;
        [TabGroup("Settings")][SerializeField] private float mono;
        
        [TabGroup("Aesthetics")]public GameObject handleVisual;
        private enum HandleMaterial { ProximityShader, Translucent };
        [TabGroup("Aesthetics")][SerializeField] private HandleMaterial mat;
        [TabGroup("Aesthetics")][SerializeField] private Material proximityShader;
        [TabGroup("Aesthetics")][SerializeField] private Material translucent;

        private Rigidbody rb;
        
        private bool lActive;
        private bool cActive;
        private bool rActive;

        public float R = 10f;
        private const float HoverHeight = 1f;
        private const float HoverForce = 7f;
        private const float RightingThreshold = 0f;
        private const float RightingForce = 2f;
        private const float UpwardForce = 1f;
        private const float UpwardHeight = 1f;

        private void Start()
        {
            rb = controller.transform.GetComponent<Rigidbody>();
            SetPosition(controller.CameraPosition(), transform.localPosition);
            switch (mat)
            {
                case HandleMaterial.Translucent:
                    SetMaterial(lHandle.HandleRenderer, translucent);
                    SetMaterial(rHandle.HandleRenderer, translucent);
                    SetMaterial(cHandle.HandleRenderer, translucent);
                    break;
                case HandleMaterial.ProximityShader:
                    SetMaterial(lHandle.HandleRenderer, proximityShader);
                    SetMaterial(rHandle.HandleRenderer, proximityShader);
                    SetMaterial(cHandle.HandleRenderer, proximityShader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SetMaterial(Renderer r, Material m)
        {
            r.material = m;
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
            
            SetHandleValues(lHandle, dual);
            SetHandleValues(rHandle, dual);
            SetHandleValues(cHandle, mono);
        }

        private static void SetHandleValues(HandleController h, float f)
        {
            h.ClipThreshold = f;
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
