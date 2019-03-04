﻿using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class HandleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms controller;
        private float minThreshold = .1f;
        private float maxThreshold = .5f;
        private const float A = 75f;
        public float ClipThreshold { private get; set; }
        
        private GameObject midpoint;
        private GameObject midpointParent;
        private Renderer r;

        private enum Handle { Left, Center, Right };
        [SerializeField] private Handle handle;
        
        private static readonly int Threshold = Shader.PropertyToID("_ClipThreshold");
        private static readonly int CutThreshold = Shader.PropertyToID("_CutThreshold");
        private static readonly int LeftHand = Shader.PropertyToID("_LeftHand");
        private static readonly int RightHand = Shader.PropertyToID("_RightHand");
        private static readonly int Activated = Shader.PropertyToID("_Activated");

        private LineRenderer lr;
        
        public bool Active { get; private set; }
        public float M { get; private set; }
        
        private void Start()
        {
           Debug.Log(name + "[1]: " + ClipThreshold + ", " + minThreshold);
           SetupThresholds();
           Debug.Log(name + "[2]: " + ClipThreshold + ", " + minThreshold);
           SetupShader();
           SetupLineRender();
           SetupMidpoint();
        }

        private void SetupThresholds()
        {
            minThreshold = ClipThreshold * .5f;
            maxThreshold = .5f;
        }
        
        private void SetupShader()
        {
            r = transform.GetComponent<Renderer>();
            
            r.material.SetFloat(Threshold, ClipThreshold * .75f);
            r.material.SetFloat(CutThreshold, ClipThreshold * 0f);
            transform.localScale = new Vector3(
                ClipThreshold, // + ClipThreshold, 
                ClipThreshold, // + ClipThreshold, 
                ClipThreshold);// + ClipThreshold);
            r.material.SetFloat(Activated, 1);
        }

        private void SetupLineRender()
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.startWidth = .005f;
            lr.endWidth = .005f;
            lr.material = controller.lineRenderMat;
        }

        private void SetupMidpoint()
        {
            midpoint = new GameObject {name = name + "_midpoint"};
            midpointParent = new GameObject {name = name + "_midpointParent"};
            midpointParent.transform.parent = null;
            midpoint.transform.parent = midpointParent.transform;
            midpoint.transform.localPosition = Vector3.zero;
            midpoint.transform.localScale = new Vector3(1,1,1);
            midpointParent.transform.localScale = new Vector3(1,1,1);
        }
        
        private void Update()
        {
            r.material.SetVector(LeftHand, controller.LeftControllerTransform().position);
            r.material.SetVector(RightHand, controller.RightControllerTransform().position);
            
            switch (handle)
            {
                case Handle.Left:
                    HandleCheck(controller.LeftControllerTransform(), controller.LeftGrab());
                    MidpointCalculation(transform, controller.LeftControllerTransform());
                    midpointParent.transform.position = controller.LeftControllerTransform().position;
                    midpointParent.transform.rotation = controller.LeftControllerTransform().rotation;
                    break;
                case Handle.Right:
                    HandleCheck(controller.RightControllerTransform(), controller.RightGrab());
                    MidpointCalculation(transform, controller.RightControllerTransform());
                    midpointParent.transform.position = controller.RightControllerTransform().position;
                    midpointParent.transform.rotation = controller.RightControllerTransform().rotation;
                    break;
                case Handle.Center:
                    break;
            }
        }

        private void MidpointCalculation(Transform a, Transform b)
        {
            float depth = Vector3.Distance(a.position, b.position) / 2;
            midpoint.transform.localPosition = Vector3.Lerp(midpoint.transform.localPosition, new Vector3(0, 0, depth), .3f);
        }
        
        private void HandleCheck(Transform c, bool g)
        {
            Debug.Log(name + ": " + M);
            
            if (_distance(c) <= ClipThreshold)
            {
                EnableDisable(g, g ? 0 : 1, c);
                M = _distance(c) <= minThreshold ? 0f : _distance(c) * A;
            }
            else
            {
                if (_distance(c) >= maxThreshold)
                {
                    EnableDisable(false, 1, c);
                    return;
                }
                EnableDisable(g, Mathf.Lerp(0, .999f, _distance(c)) / maxThreshold, c);
                if (g) return;
                EnableDisable(false, 1, c);
            }
        }
        
        private void EnableDisable(bool toggle, float value, Transform b)
        {
            r.material.SetFloat(Activated, value);
            Active = toggle;
            lr.enabled = toggle;

            if (!toggle) return;
            controller.curve.BezierLineRenderer(lr, b.position, Midpoint(), transform.position, 15);
        }
        
        private Vector3 Midpoint()
        {            
            return midpoint.transform.position;
        }
        
        private float _distance(Transform c)
        {
            return Vector3.Distance(transform.position, c.position);
        }

        private static void DebugLines(Transform a, Transform b, Color c)
        {
            Debug.DrawLine(a.position, b.position, c);
        }
    }
}

