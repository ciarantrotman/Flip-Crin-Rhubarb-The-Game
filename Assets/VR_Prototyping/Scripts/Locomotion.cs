using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class Locomotion : MonoBehaviour
    {

        private GameObject parent;
        
        private GameObject rCf; // follow
        private GameObject rCp; // proxy
        private GameObject rCn; // normalised
        private GameObject rMp; // midpoint
        private GameObject rTs; // target
        private GameObject rHp; // hit
        private GameObject rVo; // visual
        
        private GameObject lCf; // follow
        private GameObject lCp; // proxy
        private GameObject lCn; // normalised
        private GameObject lMp; // midpoint
        private GameObject lTs; // target
        private GameObject lHp; // hit
        private GameObject lVo; // visual

        [HideInInspector] public LineRenderer lLr;
        [HideInInspector] public LineRenderer rLr;
        
        private const float MaxAngle = 60f;
        private const float MinAngle = 0f;
        
        private const float Trigger = .8f;
        private const float Sensitivity = .01f;
        private const float Tolerance = .05f;

        private float directionR;
        private float pDirectionL;
        
        private bool pTouchR;
        private bool pTouchL;

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
            
            rCf = new GameObject("Locomotion/Follow/Right");
            rCp = new GameObject("Locomotion/Proxy/Right");
            rCn = new GameObject("Locomotion/Normalised/Right");
            rMp = new GameObject("Locomotion/MidPoint/Right");
            rTs = new GameObject("Locomotion/Target/Right");
            rHp = new GameObject("Locomotion/HitPoint/Right");
            
            lCf = new GameObject("Locomotion/Follow/Left");
            lCp = new GameObject("Locomotion/Proxy/Left");
            lCn = new GameObject("Locomotion/Normalised/Left");
            lMp = new GameObject("Locomotion/MidPoint/Left");
            lTs = new GameObject("Locomotion/Target/Left");
            lHp = new GameObject("LocomotionTeleport/HitPoint/Left");
            
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
            rHp.transform.SetParent(rTs.transform);
            
            lCf.transform.SetParent(p);
            lCp.transform.SetParent(lCf.transform);
            lCn.transform.SetParent(lCf.transform);
            lMp.transform.SetParent(lCp.transform);
            lTs.transform.SetParent(lCn.transform);
            lHp.transform.SetParent(lTs.transform);
            
            rLr = rCp.AddComponent<LineRenderer>();
            Setup.LineRender(rLr, lineRenderMat, .005f, false);
            
            lLr = lCp.AddComponent<LineRenderer>();
            Setup.LineRender(lLr, lineRenderMat, .005f, false);
        }

        private void Update()
        {
            Set.LocalDepth(rTs.transform, CalculateDepth(ControllerAngle(rCf, rCp, rCn, c.RightControllerTransform(), c.CameraTransform(), c.debugActive), max, min, rCp.transform), false, .2f);
            Set.LocalDepth(lTs.transform, CalculateDepth(ControllerAngle(lCf, lCp, lCn, c.LeftControllerTransform(), c.CameraTransform(), c.debugActive), max, min, lCp.transform), false, .2f);

            TargetLocation(rTs, rHp, transform);
            TargetLocation(lTs, lHp, transform);

            Set.LocalDepth(rMp.transform, Set.Midpoint(rCp.transform, rTs.transform), false, 0f);
            Set.LocalDepth(lMp.transform, Set.Midpoint(rCp.transform, rTs.transform), false, 0f);

            DrawLineRender(rLr, c.RightControllerTransform(), rMp.transform, rHp.transform, lineRenderQuality, c.RightJoystickPress(), disableRightHand);            
            DrawLineRender(lLr, c.LeftControllerTransform(), lMp.transform, lHp.transform, lineRenderQuality, c.LeftJoystickPress(), disableLeftHand);

        }

        private void LateUpdate()
        {
            //ObjectMethods.Locomotion(this, c.RightJoystickPress(), pTouchR, rVo, rHp);
            //ObjectMethods.Locomotion(this, c.LeftJoystickPress(), pTouchL, lVo, lHp);
            //pTouchR = c.RightJoystickPress();
            //pTouchL = c.LeftJoystickPress();

            if (rotation && c.RightJoystick().x > Trigger)
            {
                Debug.Log(c.RightJoystick().x + " : " + Trigger);
                StartCoroutine(RotationCheckRight(RotationAngle(angle), rotateSpeed));
            }
            //if (rotation && c.RightJoystick().x > -Trigger)
            //{
            //    Debug.Log(c.RightJoystick().x + " : " + -Trigger);
            //    //StartCoroutine(RotationCheckLeft(RotationAngle(-angle), rotateSpeed));
            //}
        }

        private Vector3 RotationAngle(float a)
        {
            var t = transform.eulerAngles;
            return new Vector3(t.x, t.y + a, t.z);
        }

        private static bool JoystickLock()
        {
            return false;
        }

        private IEnumerator RotationCheckRight(Vector3 a, float d)
        {
            yield return new WaitForSeconds(Sensitivity);
            var check = c.RightJoystick().x- 0 <= Tolerance;
            //transform.DORotate(a, d);
            transform.RotateAround(c.CameraPosition(), Vector3.up, angle);
            print(check);
        }

        public void LocomotionStart(GameObject visual)
        {
            visual.SetActive(true);
        }
        public void LocomotionStay(GameObject visual)
        {
            
        }
        public void LocomotionEnd(GameObject visual, Transform target)
        {
            transform.DOMove(target.position, moveSpeed);
            visual.SetActive(false);
        }
        
        private static void DrawLineRender(LineRenderer lr, Transform con, Transform mid, Transform end, int quality, bool touch, bool hand)
        {
            lr.enabled = touch && !hand;
            BezierCurve.BezierLineRenderer(lr,
                con.position,
                mid.position,
                end.position,
                quality);
        }
        private static float ControllerAngle(GameObject follow, GameObject proxy, GameObject normal, Transform controller, Transform head, bool debug)
        {
            //Set.Transforms(proxy.transform, controller);
            Set.Position(proxy.transform, controller);
            Set.ForwardVector(proxy.transform, controller);
            Set.SplitRotation(proxy.transform, normal.transform, true);
            Set.SplitPosition(head, controller, follow.transform);
            follow.transform.LookAt(proxy.transform);

            
            
            if (!debug) return Vector3.Angle(normal.transform.forward, proxy.transform.forward);
            
            var normalForward = normal.transform.forward;
            var proxyForward = proxy.transform.forward;
            var position = proxy.transform.position;
            
            Debug.DrawLine(follow.transform.position, position, Color.red);
            Debug.DrawRay(normal.transform.position, normalForward, Color.blue);
            Debug.DrawRay(position, proxyForward, Color.blue);

            return Vector3.Angle(normalForward, proxyForward);
        }
        private static float CalculateDepth(float angle, float max, float min, Transform proxy)
        {
            var a = angle;

            a = a > MaxAngle ? MaxAngle : a;
            a = a < MinAngle ? MinAngle : a;

            a = proxy.eulerAngles.x < 180 ? MinAngle : a;
            
            var proportion = Mathf.InverseLerp(MaxAngle, MinAngle, a);
            return Mathf.Lerp(max, min, proportion);
        }

        private static void TargetLocation(GameObject target, GameObject hitpoint, Transform current)
        {
            var t = target.transform;
            var position = t.position;
            var up = t.up;
            hitpoint.transform.position = Physics.Raycast(position, -up, out var hit) ? hit.point : current.position;
            hitpoint.transform.up = Physics.Raycast(position, -up, out var h) ? h.normal : current.transform.up;
        }
    }
}
