using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class HandleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms controller;
        [SerializeField] private float minThreshold;
        [SerializeField] private float maxThreshold;
        [HideInInspector] public float clipThreshold;
        
        private GameObject midpoint;
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
           SetupThresholds();
           SetupShader();
           SetupLineRender();
           SetupMidpoint();
           
        }

        private void SetupThresholds()
        {
            minThreshold = clipThreshold;
            maxThreshold = .5f;
        }
        
        private void SetupShader()
        {
            r = transform.GetComponent<Renderer>();
            
            r.material.SetFloat(Threshold, clipThreshold * .75f);
            r.material.SetFloat(CutThreshold, clipThreshold * 0f);
            transform.localScale = new Vector3(
                clipThreshold, // + ClipThreshold, 
                clipThreshold, // + ClipThreshold, 
                clipThreshold);// + ClipThreshold);
            r.material.SetFloat(Activated, 1);
        }

        private void SetupLineRender()
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.startWidth = .01f;
            lr.endWidth = .01f;
            lr.material = controller.lineRenderMat;
        }

        private void SetupMidpoint()
        {
            midpoint = new GameObject {name = name + " Midpoint"};
            midpoint.transform.parent = transform;
            midpoint.transform.localPosition = Vector3.zero;
            midpoint.transform.localScale = new Vector3(1,1,1);
        }
        
        private void Update()
        {
            r.material.SetVector(LeftHand, controller.LeftControllerTransform().position);
            r.material.SetVector(RightHand, controller.RightControllerTransform().position);

            switch (handle)
            {
                case Handle.Left:
                    HandleCheck(controller.LeftControllerTransform(), controller.LeftGrab());
                    break;
                case Handle.Right:
                    HandleCheck(controller.RightControllerTransform(), controller.RightGrab());
                    break;
                case Handle.Center:
                    break;
            }
        }
        
        private void HandleCheck(Transform c, bool g)
        {
            if (_distance(c) <= clipThreshold)
            {
                EnableDisable(g, g ? 0 : 1, c);
                M = _distance(c) <= minThreshold ? 0f : _distance(c) * 50f;
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

        private Vector3 Midpoint(Transform a, Transform b)
        {
            DebugLines(transform, midpoint.transform, Color.black);
            DebugLines(midpoint.transform, b, Color.black);
            
            transform.LookAt(b);
            float depth = Vector3.Distance(a.position, b.position) / 2;
            midpoint.transform.localPosition = new Vector3(0, 0, depth);
            return midpoint.transform.position;
        }
        
        private void EnableDisable(bool toggle, float value, Transform b)
        {
            Debug.Log(name + toggle);
            r.material.SetFloat(Activated, value);
            Active = toggle;
            lr.enabled = false;

            if (!toggle) return;
            var pos = transform;
            controller.curve.BezierLineRenderer(lr, pos.position, Midpoint(pos, b), b.position, 15);
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

