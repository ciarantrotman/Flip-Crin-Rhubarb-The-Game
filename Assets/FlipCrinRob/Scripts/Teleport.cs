using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class Teleport : MonoBehaviour
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

        private bool pTouchR;
        private bool pTouchL;

        private enum Method
        {
            Dash,
            Blink
        }
        
        [BoxGroup("Distance Settings")] [Range(.15f, 1f)] [SerializeField] private float min = .5f;
        [BoxGroup("Distance Settings")] [Range(1f, 100f)] [SerializeField] private float max = 15f;
        [TabGroup("Teleport Settings")] [SerializeField] private Method teleportType;
        [TabGroup("Teleport Settings")] [SerializeField] private bool disableLeftHand;
        [TabGroup("Teleport Settings")] [SerializeField] private bool disableRightHand;
        [TabGroup("Aesthetic Settings")] [SerializeField] [Required] private GameObject destination;
        [TabGroup("Aesthetic Settings")] [SerializeField] [Required] private AnimationCurve teleportEasing;
        [TabGroup("Aesthetic Settings")] [Range(0f, 1f)] [SerializeField] private float teleportSpeed = .75f;
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
            parent = new GameObject("Teleport/Calculations");
            var p = parent.transform;
            p.SetParent(transform);
            
            rCf = new GameObject("Teleport/Follow/Right");
            rCp = new GameObject("Teleport/Proxy/Right");
            rCn = new GameObject("Teleport/Normalised/Right");
            rMp = new GameObject("Teleport/MidPoint/Right");
            rTs = new GameObject("Teleport/Target/Right");
            rHp = new GameObject("Teleport/HitPoint/Right");
            
            lCf = new GameObject("Teleport/Follow/Left");
            lCp = new GameObject("Teleport/Proxy/Left");
            lCn = new GameObject("Teleport/Normalised/Left");
            lMp = new GameObject("Teleport/MidPoint/Left");
            lTs = new GameObject("Teleport/Target/Left");
            lHp = new GameObject("Teleport/HitPoint/Left");
            
            rVo = Instantiate(destination, rHp.transform);
            rVo.name = "Teleport/Visual/Right";
            rVo.SetActive(false);
            
            lVo = Instantiate(destination, lHp.transform);
            lVo.name = "Teleport/Visual/Left";
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
            Set.LocalDepth(rTs.transform, CalculateDepth(ControllerAngle(rCf, rCp, rCn, c.RightControllerTransform(), c.CameraTransform(), c.debugActive), max, min, rCp.transform));
            Set.LocalDepth(lTs.transform, CalculateDepth(ControllerAngle(lCf, lCp, lCn, c.LeftControllerTransform(), c.CameraTransform(), c.debugActive), max, min, lCp.transform));

            TeleportLocation(rTs, rHp, transform);
            TeleportLocation(lTs, lHp, transform);

            Set.LocalDepth(rMp.transform, Set.Midpoint(rCp.transform, rTs.transform));
            Set.LocalDepth(lMp.transform, Set.Midpoint(rCp.transform, rTs.transform));

            DrawLineRender(rLr, c.RightControllerTransform(), rMp.transform, rHp.transform, lineRenderQuality, c.RightTouchpad(), disableRightHand);            
            DrawLineRender(lLr, c.LeftControllerTransform(), lMp.transform, lHp.transform, lineRenderQuality, c.LeftTouchpad(), disableLeftHand);

        }

        private void LateUpdate()
        {
            ObjectMethods.Teleport(this, c.RightTouchpad(), pTouchR, rVo, rHp);
            ObjectMethods.Teleport(this, c.LeftTouchpad(), pTouchL, lVo, lHp);
            pTouchR = c.RightTouchpad();
            pTouchL = c.LeftTouchpad();
        }

        public void TeleportStart(GameObject visual)
        {
            visual.SetActive(true);
        }
        public void TeleportStay(GameObject visual)
        {
            
        }
        public void TeleportEnd(GameObject visual, Transform target)
        {
            transform.DOMove(target.position, teleportSpeed);
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

        private static void TeleportLocation(GameObject target, GameObject hitpoint, Transform current)
        {
            var t = target.transform;
            var position = t.position;
            var up = t.up;
            hitpoint.transform.position = Physics.Raycast(position, -up, out var hit) ? hit.point : current.position;
            hitpoint.transform.up = Physics.Raycast(position, -up, out var h) ? h.normal : current.transform.up;
        }
    }
}
