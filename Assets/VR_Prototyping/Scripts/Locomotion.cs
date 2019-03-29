using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.PostProcessing;
using Valve.VR;

namespace VR_Prototyping.Scripts
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class Locomotion : MonoBehaviour
    {

        private GameObject parent;
        private GameObject cN;  // camera normalised
        
        private GameObject rCf; // follow
        private GameObject rCp; // proxy
        private GameObject rCn; // normalised
        private GameObject rMp; // midpoint
        private GameObject rTs; // target
        private GameObject rHp; // hit
        private GameObject rVo; // visual
        private GameObject rRt; // rotation
        
        private GameObject lCf; // follow
        private GameObject lCp; // proxy
        private GameObject lCn; // normalised
        private GameObject lMp; // midpoint
        private GameObject lTs; // target
        private GameObject lHp; // hit
        private GameObject lVo; // visual
        private GameObject lRt; // rotation

        [HideInInspector] public LineRenderer lLr;
        [HideInInspector] public LineRenderer rLr;
        
        private const float MaxAngle = 60f;
        private const float MinAngle = 0f;
        
        private const float Trigger = .85f;
        private const float Sensitivity = 10f;
        private const float Tolerance = .1f;
        
        private readonly List<Vector2> rJoystickValues = new List<Vector2>();
        private readonly List<Vector2> lJoystickValues = new List<Vector2>();
        
        private bool pTouchR;
        private bool pTouchL;

        private Vector3 rRotTarget;
        
        private bool active;

        private enum Method
        {
            Dash,
            Blink
        }
        
        [BoxGroup("Distance Settings")] [Range(.15f, 1f)] [SerializeField] private float min = .5f;
        [BoxGroup("Distance Settings")] [Range(1f, 100f)] [SerializeField] private float max = 15f;
        [TabGroup("Locomotion Settings")] [SerializeField] private Method locomotionMethod;
        [TabGroup("Locomotion Settings")] [SerializeField] private bool rotation;
        [TabGroup("Locomotion Settings")] [ShowIf("rotation")] [Indent] [SerializeField] private float angle;
        [TabGroup("Locomotion Settings")] [ShowIf("rotation")] [Indent] [Range(0f, 1f)] [SerializeField] private float rotateSpeed = .15f;
        [TabGroup("Locomotion Settings")] [SerializeField] private bool disableLeftHand;
        [TabGroup("Locomotion Settings")] [SerializeField] private bool disableRightHand;
        [TabGroup("Aesthetic Settings")] [Range(0f, 1f)] [SerializeField] private float moveSpeed = .75f;
        [TabGroup("Aesthetic Settings")] [Space(5)] [SerializeField] [Required] private GameObject targetVisual;
        [TabGroup("Aesthetic Settings")] [SerializeField] [Required] private AnimationCurve locomotionEasing;
        [TabGroup("Aesthetic Settings")] [SerializeField] [Required] private Material lineRenderMat;
        [TabGroup("Aesthetic Settings")] [Range(3f, 50f)] [SerializeField] private int lineRenderQuality = 40;
        [TabGroup("Aesthetic Settings")] [SerializeField] private PostProcessVolume postProcessing;
        
        private ControllerTransforms c;
        
        private void Start()
        {
            c = GetComponent<ControllerTransforms>();
            SetupGameObjects();
        }

        private void SetupGameObjects()
        {
            parent = new GameObject("Locomotion/Calculations");
            var p = parent.transform;
            p.SetParent(transform);
            
            cN = new GameObject("Locomotion/Temporary");
            
            rCf = new GameObject("Locomotion/Follow/Right");
            rCp = new GameObject("Locomotion/Proxy/Right");
            rCn = new GameObject("Locomotion/Normalised/Right");
            rMp = new GameObject("Locomotion/MidPoint/Right");
            rTs = new GameObject("Locomotion/Target/Right");
            rHp = new GameObject("Locomotion/HitPoint/Right");
            rRt = new GameObject("Locomotion/Rotation/Right");
            
            lCf = new GameObject("Locomotion/Follow/Left");
            lCp = new GameObject("Locomotion/Proxy/Left");
            lCn = new GameObject("Locomotion/Normalised/Left");
            lMp = new GameObject("Locomotion/MidPoint/Left");
            lTs = new GameObject("Locomotion/Target/Left");
            lHp = new GameObject("Locomotion/HitPoint/Left");
            lRt = new GameObject("Locomotion/Rotation/Left");
            
            rVo = Instantiate(targetVisual, rHp.transform);
            rVo.name = "Locomotion/Visual/Right";
            rVo.SetActive(false);
            
            lVo = Instantiate(targetVisual, lHp.transform);
            lVo.name = "Locomotion/Visual/Left";
            lVo.SetActive(false);
            
            rCf.transform.SetParent(p);
            rCp.transform.SetParent(rCf.transform);
            rCn.transform.SetParent(rCf.transform);
            rMp.transform.SetParent(rCp.transform);
            rTs.transform.SetParent(rCn.transform);
            rHp.transform.SetParent(transform);
            rRt.transform.SetParent(rHp.transform);
            
            lCf.transform.SetParent(p);
            lCp.transform.SetParent(lCf.transform);
            lCn.transform.SetParent(lCf.transform);
            lMp.transform.SetParent(lCp.transform);
            lTs.transform.SetParent(lCn.transform);
            lHp.transform.SetParent(transform);
            lRt.transform.SetParent(lHp.transform);
            
            rLr = rCp.AddComponent<LineRenderer>();
            Setup.LineRender(rLr, lineRenderMat, .005f, false);
            
            lLr = lCp.AddComponent<LineRenderer>();
            Setup.LineRender(lLr, lineRenderMat, .005f, false);
        }

        private void Update()
        {
            Set.LocalDepth(rTs.transform, Check.CalculateDepth(Check.ControllerAngle(rCf, rCp, rCn, c.RightControllerTransform(), c.CameraTransform(), c.debugActive), MaxAngle, MinAngle, max, min, rCp.transform), false, .2f);
            Set.LocalDepth(lTs.transform, Check.CalculateDepth(Check.ControllerAngle(lCf, lCp, lCn, c.LeftControllerTransform(), c.CameraTransform(), c.debugActive), MaxAngle, MinAngle, max, min, lCp.transform), false, .2f);

            Check.TargetLocation(rTs, rHp, transform);
            Check.TargetLocation(lTs, lHp, transform);

            Set.LocalDepth(rMp.transform, Set.Midpoint(rCp.transform, rTs.transform), false, 0f);
            Set.LocalDepth(lMp.transform, Set.Midpoint(lCp.transform, lTs.transform), false, 0f);
            
            Check.Target(rVo, rHp, rCn.transform, c.RightJoystick(), rRt);
            Check.Target(lVo, lHp, lCn.transform, c.LeftJoystick(), lRt);
            
            BezierCurve.BezierLineRenderer(rLr,c.RightControllerTransform().position,rMp.transform.position,rHp.transform.position,lineRenderQuality);
            BezierCurve.BezierLineRenderer(lLr, c.LeftControllerTransform().position, lMp.transform.position, lHp.transform.position, lineRenderQuality);

        }

        private void LateUpdate()
        {
            Check.JoystickTracking(rJoystickValues, c.RightJoystick(), Sensitivity);
            Check.JoystickTracking(lJoystickValues, c.LeftJoystick(), Sensitivity);
            
            Check.GestureDetection(this, c.RightJoystick(), rJoystickValues[0], angle, rotateSpeed, Trigger, Tolerance, rVo, rLr, c.RightJoystickPress(), pTouchR, disableRightHand, active);
            Check.GestureDetection(this, c.LeftJoystick(), lJoystickValues[0], angle, rotateSpeed, Trigger, Tolerance, lVo, lLr, c.LeftJoystickPress(), pTouchL, disableLeftHand, active);
            
            pTouchR = c.RightJoystickPress();
            pTouchL = c.LeftJoystickPress();
        }

        private static Vector3 RotationAngle(Transform target, float a)
        {
            var t = target.eulerAngles;
            return new Vector3(t.x, t.y + a, t.z);
        }

        public void RotateUser(float a, float time)
        {
            if(transform.parent == cN.transform || !rotation) return;
            active = true;
            
            Set.SplitRotation(c.CameraTransform(), cN.transform, false);
            Set.SplitPosition(c.CameraTransform(), transform, cN.transform);
            
            transform.SetParent(cN.transform);
            cN.transform.DORotate(RotationAngle(cN.transform, a), time);
            StartCoroutine(Uncouple(transform, time));
        }

        public void LocomotionStart(GameObject visual, LineRenderer lr)
        {
            visual.SetActive(true);
            lr.enabled = true;
            active = true;
        }
        
        public void LocomotionEnd(GameObject visual, Vector3 posTarget, Vector3 rotTarget, LineRenderer lr)
        {
            if (transform.parent == cN.transform) return;

            Set.SplitRotation(c.CameraTransform(), cN.transform, false);
            Set.SplitPosition(c.CameraTransform(), transform, cN.transform);
            
            transform.SetParent(cN.transform);
            Debug.Log(rotTarget);
            switch (locomotionMethod)
            {
                case Method.Dash:
                    cN.transform.DOMove(posTarget, moveSpeed);
                    cN.transform.DORotate(rotTarget, moveSpeed);
                    StartCoroutine(Uncouple(transform, moveSpeed));
                    break;
                case Method.Blink:
                    cN.transform.position = posTarget;
                    cN.transform.eulerAngles = rotTarget;
                    transform.SetParent(null);
                    active = false;
                    break;
                default:
                    throw new ArgumentException();
            }
            
            visual.SetActive(false);
            lr.enabled = false;
        }

        private IEnumerator Uncouple(Transform a, float time)
        {
            yield return new WaitForSeconds(time);
            a.SetParent(null);
            active = false;
            yield return null;
        }
    }
}
