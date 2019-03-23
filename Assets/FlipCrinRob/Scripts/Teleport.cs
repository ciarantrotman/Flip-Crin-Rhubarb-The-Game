using UnityEngine;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class Teleport : MonoBehaviour
    {
        private const float Min = .15f;
        private const float Max = .65f;
        private GameObject parent;
        
        private GameObject lH;
        private GameObject rH;
        private GameObject lC;
        private GameObject rC;
        private GameObject lM;
        private GameObject lS;
        private GameObject rM;
        private GameObject rS;

        private GameObject cL;
        private GameObject lN;
        private GameObject lP;
        private GameObject lA;
        private GameObject lX;
        private GameObject lF;

        private GameObject cR;
        private GameObject rN;
        private GameObject rP;
        private GameObject rA;
        private GameObject rX;
        private GameObject rF;
        
        private ControllerTransforms controller;
        
        private void Start()
        {
            SetupGameObjects();
            controller = GetComponent<ControllerTransforms>();
        }

        private void SetupGameObjects()
        {
            parent = new GameObject("TeleportGameobjects");
            var p = parent.transform;
            
            lH = new GameObject("lH");
            lH.transform.SetParent(p);
            
            rH = new GameObject("rH");
            rH.transform.SetParent(p);
            
            lC = new GameObject("lC");
            lC.transform.SetParent(lH.transform);
            
            rC = new GameObject("rC");
            rC.transform.SetParent(rH.transform);
            
            lM = new GameObject("lM");
            lM.transform.SetParent(lH.transform);
            
            rM = new GameObject("rM");
            rM.transform.SetParent(rH.transform);
            
            lS = new GameObject("lS");
            lS.transform.SetParent(lH.transform);
            
            rS = new GameObject("rS");
            rS.transform.SetParent(rH.transform);
            
            cL = new GameObject("cL");
            cL.transform.SetParent(p);
            
            lN = new GameObject("lN");
            lN.transform.SetParent(p);
            
            cR = new GameObject("cR");
            cR.transform.SetParent(p);
            
            rN = new GameObject("rN");
            rN.transform.SetParent(p);
            
            lP = new GameObject("lP");
            lP.transform.SetParent(p);
            
            rP = new GameObject("rP");
            rP.transform.SetParent(p);
            
            lA = new GameObject("lA");
            lA.transform.SetParent(p);
            
            rA = new GameObject("rA");
            rA.transform.SetParent(p);
            
            lX = new GameObject("lX");
            lX.transform.SetParent(p); 
            
            rX = new GameObject("rX");
            rX.transform.SetParent(p);
            
            lF = new GameObject("lF");
            lF.transform.SetParent(p);
            
            rF = new GameObject("rF");
            rF.transform.SetParent(p);
        }

        private void Update()
        {
            SetTransform(lH, lC, lM, lS, controller.CameraTransform(), controller.LeftControllerTransform(), controller.debugActive);
            SetTransform(rH, rC, rM, rS,controller.CameraTransform(), controller.RightControllerTransform(), controller.debugActive);
            CalculateCenter(controller.CameraTransform(), controller.LeftControllerTransform(), cL, lN, lP, lA, lX, lF, controller.debugActive);
            CalculateCenter(controller.CameraTransform(), controller.RightControllerTransform(), cR, rN, rP, rA, rX, rF, controller.debugActive);
        }

        private static void SetTransform(GameObject G, GameObject H, GameObject M, GameObject S, Transform C, Transform T, bool d)
        {
            var c = C.position;
            var t = T.position;
            var g = G.transform.position;
            var s = S.transform.position;
            var z = H.transform.localPosition.z;
            
            var x = new Vector3(c.x, t.y, c.z);
            var m = Mathf.Pow(Mathf.InverseLerp(Min, Max, z), 2f);
           
            G.transform.position = x;
            G.transform.LookAt(t);
            H.transform.position = t;
            M.transform.localPosition = new Vector3(0,0,z * m);
            S.transform.position = new Vector3(c.x, 0, c.z);
            S.transform.LookAt(C);
            
            #region Debug Lines

            if (!d) return;
            
            Debug.DrawLine(g, t, Color.cyan);
            Debug.DrawRay(M.transform.position, M.transform.up * m, Color.cyan);
            Debug.DrawRay(g, G.transform.up * .25f, Color.green);
            Debug.DrawRay(g, G.transform.right * .25f, Color.red);
            Debug.DrawRay(g, G.transform.forward * .25f, Color.blue);
            
            Debug.DrawRay(s, S.transform.forward, Color.magenta);

            #endregion
            
        }
        private static void CalculateCenter(Component cam, Transform con, GameObject C, GameObject N, GameObject P, GameObject A, GameObject X, GameObject F, bool d)
        {
            Set.Transforms(C.transform, con);
            var c = C.transform.position;
            var r = C.transform.eulerAngles;
            var n = cam.transform.position;
            
            var cN = new Vector3(c.x, 0, c.z);
            var cR = new Vector3(0, r.y, 0);
            var nC = new Vector3(n.x, 0f, n.z);
            
            var i = Intersection.Line(nC, P.transform.right, cN, N.transform.forward, .1f);
            X.transform.position = i;
            var x = Intersection.Line(i, X.transform.up, c, C.transform.forward, .1f);
            var iA = new Vector3(i.x, c.y, i.z);
            var f = new Vector3(n.x, x.y, n.z);
            
            N.transform.position = cN;
            N.transform.eulerAngles = cR;
            P.transform.position = nC;
            P.transform.eulerAngles = cR;
            A.transform.position = iA;

            F.transform.position = f;
            F.transform.LookAt(con);
            
            #region Debug Lines

            if (!d) return;

            Debug.DrawLine(c, x, Color.blue);
            Debug.DrawLine(cN, i, Color.blue);
            Debug.DrawLine(c, cN, Color.cyan);            
            Debug.DrawLine(nC, i, Color.red);
            Debug.DrawLine(i, x, Color.green);
            Debug.DrawLine(f, c, Color.yellow);
            
            #endregion
        }
    }
}
