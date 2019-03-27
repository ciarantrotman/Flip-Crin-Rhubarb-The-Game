using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;

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
        
        private const float Trigger = .65f;
        private const float Sensitivity = .01f;
        private const float Tolerance = .05f;
        
        private bool pTouchR;
        private bool pTouchL;
        
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
            rHp.transform.SetParent(rTs.transform);
            rRt.transform.SetParent(rHp.transform);
            
            lCf.transform.SetParent(p);
            lCp.transform.SetParent(lCf.transform);
            lCn.transform.SetParent(lCf.transform);
            lMp.transform.SetParent(lCp.transform);
            lTs.transform.SetParent(lCn.transform);
            lHp.transform.SetParent(lTs.transform);
            lRt.transform.SetParent(lHp.transform);
            
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

            DrawLineRender(rLr, c.RightControllerTransform(), rMp.transform, rHp.transform, lineRenderQuality);            
            DrawLineRender(lLr, c.LeftControllerTransform(), lMp.transform, lHp.transform, lineRenderQuality);
            
            Target(rVo, rHp, rCn.transform, c.RightJoystick(), rRt);
            Target(lVo, lHp, lCn.transform, c.LeftJoystick(), lRt);
        }

        private static void Target(GameObject visual, GameObject parent, Transform normal, Vector2 pos, GameObject target)
        {
            visual.transform.LookAt(RotationTarget(pos, target));
            parent.transform.forward = normal.forward;
        }
        
        private static Transform RotationTarget(Vector2 pos, GameObject target)
        {
            target.transform.localPosition = Math.Abs(pos.x) > Trigger || Math.Abs(pos.y) > Trigger ? Vector3.Lerp(target.transform.localPosition, new Vector3(pos.x, 0, pos.y), .1f) : Vector3.forward;
            return target.transform;
        }

        private void LateUpdate()
        {
            //Check.Locomotion(this, c.RightJoystickPress(), pTouchR, rVo, rLr);
            //Check.Locomotion(this, c.LeftJoystickPress(), pTouchL, lVo, lLr);
            //pTouchR = c.RightJoystickPress();
            //pTouchL = c.LeftJoystickPress();
            
            Debug.Log("1: " + c.RightJoystick().x + ", " + c.RightJoystick().y);
            
            return;
            
            if (!active && (Mathf.Abs(c.RightJoystick().x) > Trigger || Mathf.Abs(c.RightJoystick().y) > Trigger))
            {
                Debug.Log("1: " + c.RightJoystick().x + ", " + c.RightJoystick().y);
                StartCoroutine(RightGestureCheck());
            }
            else if (active && Mathf.Abs(c.RightJoystick().x) - 0 <= Tolerance && Mathf.Abs(c.RightJoystick().y) - 0 <= Tolerance)
            {
                LocomotionEnd(rVo, rLr);
            }
            
            if (!active && (Mathf.Abs(c.LeftJoystick().x) > Trigger || Mathf.Abs(c.LeftJoystick().y) > Trigger))
            {
                StartCoroutine(LeftGestureCheck());
            }
            else if (active && Mathf.Abs(c.LeftJoystick().x) - 0 <= Tolerance && Mathf.Abs(c.LeftJoystick().y) - 0 <= Tolerance)
            {
                LocomotionEnd(lVo, lLr);
            }
        }
        
        private IEnumerator RightGestureCheck()
        {
            var x = c.RightJoystick().x;
            var y = c.RightJoystick().y;
            
            Debug.Log("2: " + x + ", " + y);
            
            yield return new WaitForSeconds(Sensitivity);
            
            Debug.Log("3: " + Sensitivity);
            
            if(Mathf.Abs(c.RightJoystick().x) - 0 <= Tolerance && Mathf.Abs(c.RightJoystick().y) - 0 <= Tolerance)
            {
                if (x > Tolerance)
                {
                    Debug.Log("4: " + x + ", " + c.RightJoystick().x);
                    RotateUser(angle, rotateSpeed);
                }
                else if (x < -Tolerance)
                {
                    Debug.Log("4: " + x + ", " + c.RightJoystick().x);
                    RotateUser(-angle, rotateSpeed);
                }
                else if (y < -Tolerance)
                {
                    Debug.Log("4: " + y + ", " + c.RightJoystick().y);
                    RotateUser(180f, rotateSpeed);
                }
            }
            else if (Mathf.Abs(c.RightJoystick().x) >= Trigger && Mathf.Abs(c.RightJoystick().y) >= Trigger)
            {
                Debug.Log("4: " + x + ", " + c.RightJoystick().x + " / " + y + ", " + c.RightJoystick().y);
                
                LocomotionStart(rVo, rLr);
            }
            
            yield return null;
        }
        
        private IEnumerator LeftGestureCheck()
        {
            var x = c.LeftJoystick().x;
            var y = c.LeftJoystick().y;
            
            yield return new WaitForSeconds(Sensitivity);
            
            if(Mathf.Abs(c.LeftJoystick().x) - 0 <= Tolerance && Mathf.Abs(c.LeftJoystick().y) - 0 <= Tolerance)
            {
                if (x > Tolerance)
                {
                    RotateUser(angle, rotateSpeed);
                }
                else if (x < -Tolerance)
                {
                    RotateUser(-angle, rotateSpeed);
                }
                else if (y < -Tolerance)
                {
                    RotateUser(180f, rotateSpeed);
                }
            }
            else if (Mathf.Abs(c.LeftJoystick().x) >= Trigger && Mathf.Abs(c.LeftJoystick().y) >= Trigger)
            {
                LocomotionStart(lVo, lLr);
            }
            
            yield return null;
        }

        private static Vector3 RotationAngle(Transform target, float a)
        {
            var t = target.eulerAngles;
            return new Vector3(t.x, t.y + a, t.z);
        }

        private void RotateUser(float a, float time)
        {
            if(transform.parent == cN.transform || !rotation) return;
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
        public void LocomotionStay(GameObject visual)
        {
            
        }
        public void LocomotionEnd(GameObject visual, LineRenderer lr)
        {
            if (transform.parent == cN.transform) return;
            
            var v = visual.transform;
            
            Set.SplitRotation(c.CameraTransform(), cN.transform, false);
            Set.SplitPosition(c.CameraTransform(), transform, cN.transform);
            transform.SetParent(cN.transform);
            
            switch (locomotionMethod)
            {
                case Method.Dash:
                    cN.transform.DOMove(v.position, moveSpeed);
                    cN.transform.DORotate(v.eulerAngles, moveSpeed);
                    StartCoroutine(Uncouple(transform, moveSpeed));
                    break;
                case Method.Blink:
                    cN.transform.position = v.position;
                    cN.transform.rotation = v.rotation;
                    transform.SetParent(null);
                    break;
                default:
                    throw new ArgumentException();
            }
            
            visual.SetActive(false);
            lr.enabled = false;
            active = false;
        }

        private static IEnumerator Uncouple(Transform a, float time)
        {
            yield return new WaitForSeconds(time);
            a.SetParent(null);
            yield return null;
        }

        private static void DrawLineRender(LineRenderer lr, Transform con, Transform mid, Transform end, int quality)
        {
            BezierCurve.BezierLineRenderer(lr,
                con.position,
                mid.position,
                end.position,
                quality);
        }
        private static float ControllerAngle(GameObject follow, GameObject proxy, GameObject normal, Transform controller, Transform head, bool debug)
        {
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

        private static void TargetLocation(GameObject target, GameObject hitPoint, Transform current)
        {
            var t = target.transform;
            var position = t.position;
            var up = t.up;
            hitPoint.transform.position = Physics.Raycast(position, -up, out var hit) ? hit.point : current.position;
            hitPoint.transform.up = Physics.Raycast(position, -up, out var h) ? h.normal : current.transform.up;
        }
    }
}
