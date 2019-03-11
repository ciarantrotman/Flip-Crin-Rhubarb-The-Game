using System;
using System.Net;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class HandleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms controller;
        [SerializeField] private VehicleController vehicle;
        private float minThreshold = .1f;
        private float maxThreshold = .5f;
        public float ClipThreshold { private get; set; }
        private float clipThreshold;
        private const float DirectThreshold = .1f;
        
        private GameObject midpoint;
        private GameObject midpointParent;
        private Renderer r;

        private enum Handle { Left, Center, Right };
        [SerializeField] private Handle handle;
        private GameObject handleVisual;
        private const float LerpSpeed = .7f;
        
        private static readonly int Threshold = Shader.PropertyToID("_ClipThreshold");
        private static readonly int CutThreshold = Shader.PropertyToID("_CutThreshold");
        private static readonly int LeftHand = Shader.PropertyToID("_LeftHand");
        private static readonly int RightHand = Shader.PropertyToID("_RightHand");
        private static readonly int Activated = Shader.PropertyToID("_Activated");

        private LineRenderer lr;
        private LineRenderer vlr;
        
        public bool Active { get; private set; }
        public float M { get; private set; }
        private const float A = 100f;
        
        private void Start()
        {
           SetupThresholds();
           SetupShader();
           SetupLineRender(gameObject);
           SetupMidpoint();
           SetupVisual();
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
        private void SetupLineRender(GameObject parent)
        {
            lr = parent.AddComponent<LineRenderer>();
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
        private void SetupVisual()
        {
            handleVisual = Instantiate(vehicle.HandleVisual);
            handleVisual.transform.position = transform.position;
            handleVisual.name = name + "_Visual";
            vlr = handleVisual.AddComponent<LineRenderer>();
            vlr.startWidth = .005f;
            vlr.endWidth = .005f;
            vlr.material = controller.lineRenderMat;
        }
        private void Update()
        {
            if (controller.LeftGrab())
            {
                Haptics.Constant(controller.haptic, 150, 75, controller.LeftSource());
            }
            r.material.SetVector(LeftHand, controller.LeftControllerTransform().position);
            r.material.SetVector(RightHand, controller.RightControllerTransform().position);
            
            switch (handle)
            {
                case Handle.Left:
                    HandleCheck(controller.LeftControllerTransform(), controller.LeftGrab(), controller.LeftSource());
                    MidpointCalculation(transform, controller.LeftControllerTransform());
                    Visual(controller.LeftControllerTransform(), handleVisual.transform, controller.LeftGrab());
                    SetTransform.Follow(midpointParent.transform, controller.LeftControllerTransform());
                    break;
                case Handle.Right:
                    HandleCheck(controller.RightControllerTransform(), controller.RightGrab(), controller.RightSource());
                    MidpointCalculation(transform, controller.RightControllerTransform());
                    SetTransform.Follow(midpointParent.transform, controller.RightControllerTransform());
                    Visual(controller.RightControllerTransform(), handleVisual.transform, controller.RightGrab());
                    break;
                case Handle.Center:
//                    HandleCheck(controller.LeftControllerTransform(), controller.LeftGrab());
//                    HandleCheck(controller.RightControllerTransform(), controller.RightGrab());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void MidpointCalculation(Transform a, Transform b)
        {
            var depth = Vector3.Distance(a.position, b.position) / 2;
            midpoint.transform.localPosition = new Vector3(0, 0, depth);
        }
        private void HandleCheck(Transform c, bool g, SteamVR_Input_Sources source)
        {
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
                M = _distance(c) <= minThreshold ? 0f : _distance(c) * A;
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
            BezierCurve.BezierLineRenderer(lr, b.position, Midpoint(), transform.position, 15);
        }
        private void Visual(Transform c, Transform x, bool g)
        {
            if (_distance(c) <= clipThreshold && !g)
            {
                x.position = Vector3.Lerp(x.position, c.position, LerpSpeed);
                if (Vector3.Distance(x.position, c.position) <= DirectThreshold)
                {
                    clipThreshold = ClipThreshold + .2f;
                }
            }
            else
            {
                clipThreshold = ClipThreshold + .1f;
                x.position = Vector3.Lerp(x.position, transform.position, LerpSpeed);
            }
            vlr.SetPosition(0, x.position);
            vlr.SetPosition(1, transform.position);
        }
        private Vector3 Midpoint()
        {            
            return midpoint.transform.position;
        }
        private float _distance(Transform c)
        {
            return Vector3.Distance(transform.position, c.position);
        }
    }
}

