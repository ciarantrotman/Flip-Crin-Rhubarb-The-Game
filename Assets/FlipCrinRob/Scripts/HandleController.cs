using System;
using UnityEngine;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class HandleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms _controller;
        [HideInInspector] public float ClipThreshold;
        private Renderer _r;
        [SerializeField] private float _minThreshold;
        [SerializeField] private float _maxThreshold;
        private GameObject midpoint;
        
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
            _minThreshold = ClipThreshold;
            _maxThreshold = .5f;
        }
        
        private void SetupShader()
        {
            _r = transform.GetComponent<Renderer>();
            
            _r.material.SetFloat(Threshold, ClipThreshold * .75f);
            _r.material.SetFloat(CutThreshold, ClipThreshold * 0f);
            transform.localScale = new Vector3(
                ClipThreshold, // + ClipThreshold, 
                ClipThreshold, // + ClipThreshold, 
                ClipThreshold);// + ClipThreshold);
        }

        private void SetupLineRender()
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.startWidth = .01f;
            lr.endWidth = .01f;
            lr.material = _controller.lineRenderMat;
        }

        private void SetupMidpoint()
        {
            midpoint = new GameObject {name = name + " Midpoint"};
            midpoint.transform.parent = transform;
            midpoint.transform.localPosition = Vector3.zero;
        }
        
        private void Update()
        {
            _r.material.SetVector(LeftHand, _controller.LeftControllerTransform().position);
            _r.material.SetVector(RightHand, _controller.RightControllerTransform().position);

            HandleCheck(_controller.LeftControllerTransform(), _controller.LeftGrab());
            HandleCheck(_controller.RightControllerTransform(), _controller.RightGrab());
        }
        
        private void HandleCheck(Transform controller, bool grab)
        {
            if (_distance(controller) <= ClipThreshold)
            {
                if (_distance(controller) >= _maxThreshold)
                {
                    EnableDisable(false, 1);
                }
                EnableDisable(true, 0);
                var pos = transform;
                _controller.curve.BezierLineRenderer(lr, pos.position, Midpoint(pos, controller), controller.position, 10);
                M = _distance(controller) <= _minThreshold ? 0f : _distance(controller);
            }
            else
            {
                if (grab) return; // keep accelerating while grabbing
                EnableDisable(false, 1);
            }
        }

        private Vector3 Midpoint(Transform a, Transform b)
        {
            float depth = Vector3.Distance(a.position, b.position) / 2;
            midpoint.transform.localPosition = new Vector3(0, 0, depth);
            return midpoint.transform.position;
        }
        
        private void EnableDisable(bool toggle, int value)
        {
            _r.material.SetInt(Activated, value);
            Active = toggle;
            lr.enabled = toggle;
        }
        
        private float _distance(Transform c)
        {
            return Vector3.Distance(transform.position, c.position);
        }
    }
}

