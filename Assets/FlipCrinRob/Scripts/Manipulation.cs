using System;
using System.Xml.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(ObjectSelection))]
	public class Manipulation : MonoBehaviour
	{
		#region Inspector and Variables
		private ObjectSelection c;
		private GameObject cP;
		private GameObject oP;	
		private GameObject cO;
		private GameObject oO;
		private GameObject fM;
		private GameObject player;
		private GameObject t;
		private GameObject cF;
		private LineRenderer lr;
		private float initialDistance;
		private float m;
		private float z;
		private const float Buffer = .15f;
		
		[HideInInspector] public GameObject tS;
		
		public enum ManipulationType
		{
			Lerp,
			Physics
		}
		[TabGroup("Indirect Manipulation")] public ManipulationType manipulationType;
		[TabGroup("Indirect Manipulation")] [SerializeField] private bool objectMovesWithYouTeleport;
		[TabGroup("Indirect Manipulation")] [SerializeField] public bool rHandDisable;
		[TabGroup("Indirect Manipulation")] [SerializeField] public bool lHandDisable;
		
		[TabGroup("Snap Settings")] [SerializeField] private bool distanceSnapping;
		[TabGroup("Snap Settings")] [ShowIf("distanceSnapping")] [Range(0f, 180f)] [Indent] [SerializeField] private float snapDistance = 1.5f;
		[TabGroup("Snap Settings")] [SerializeField] private bool maximumDistance;
		
		#endregion
		private void Start () 
		{
			SetupGameObjects();
			c = GetComponent<ObjectSelection>();
			player = c.gameObject;
			lr = cP.gameObject.AddComponent<LineRenderer>();
			Setup.LineRender(lr, c.Controller.lineRenderMat, .005f, false);
			
			if (objectMovesWithYouTeleport)
			{
				fM.transform.parent = player.transform;
			}	
		}

		private void SetupGameObjects()
		{
			t = new GameObject("Manipulation/Target");
			
			fM = new GameObject("Manipulation/Manipulation");
			
			cF = new GameObject("Manipulation/Controller/Follow");
			cF.transform.SetParent(fM.transform);
			
			cO = new GameObject("Manipulation/Controller/Original");
			cO.transform.SetParent(cF.transform);
			
			cP = new GameObject("Manipulation/Controller/Proxy");
			cP.transform.SetParent(cF.transform);
			
			tS = new GameObject("Manipulation/Target/Scaled");
			tS.transform.SetParent(cP.transform);
			
			oP = new GameObject("Manipulation/Object/Proxy");
			oP.transform.SetParent(cP.transform);
			
			oO = new GameObject("Manipulation/Object/Original");
			oO.transform.SetParent(cP.transform);
		}
		private void Update()
		{	
			DrawDebugLines();
			
			Set.SplitPosition(c.Controller.CameraTransform(), c.Controller.LeftControllerTransform(), cF.transform);

			if(c.disableSelection) return;
			
			if (c.lFocusObject != null)
			{
				FocusObjectFollow(c.lFocusObject, c.LStay, c.RStay, c.lFocusObject.transform, c.Controller.LeftControllerTransform().position);
			}

			if (c.rFocusObject != null)
			{
				FocusObjectFollow(c.rFocusObject, c.LStay, c.RStay, c.rFocusObject.transform, c.Controller.RightControllerTransform().position);
			}
		}
		public void OnStart(Transform con)
		{			
			ObjectMethods.GrabStart(cF, cP, t, cO, con);
			Set.Transforms(t.transform, c.grabObject.transform);
			Set.Transforms(tS.transform, t.transform);
			Set.Position(oP.transform, c.grabObject.transform);
			Set.Position(oO.transform, c.grabObject.transform);
			lr.enabled = true;
		}

		public void OnStay(Transform con, Transform mid, Transform grabObject, int q)
		{
			ControllerFollowing(con);
			ObjectMethods.GrabLineRenderer(lr, con, mid, grabObject, q);
			tS.transform.localPosition = new Vector3(0, 0, MagnifiedDepth(cP, cO, oO, tS, snapDistance, c.selectionRange - Buffer, maximumDistance));
		}

		private static float MagnifiedDepth(GameObject conP, GameObject conO, GameObject objO, GameObject objP, float snapDistance, float max, bool limit)
		{
			var depth = conP.transform.localPosition.z / conO.transform.localPosition.z;
			var distance = Vector3.Distance(objO.transform.position, objP.transform.position);
				
			if (distance >= max && limit) return max;
			if (distance < snapDistance) return objO.transform.localPosition.z * Mathf.Pow(depth, 2);														
			return objO.transform.localPosition.z * Mathf.Pow(depth, 2.5f);
		}
		
		public void OnEnd()
		{
			lr.enabled = false;
			t.transform.SetParent(null);
		}

		private void FocusObjectFollow(Object s, bool l, bool r, Transform f, Vector3 p)
		{
			if (s == null || l || r) return;
			
			var pos = f.position;
			var rot = f.rotation;
				
			t.transform.position = pos;
			t.transform.rotation = rot;
				
			tS.transform.position = t.transform.position;
			tS.transform.rotation = t.transform.rotation;
				
			oO.transform.position = pos;
			oO.transform.rotation = rot;
				
			cO.transform.position = p;
				
			oP.transform.position = pos;
		}
		private void ControllerFollowing(Transform con)
		{
			cF.transform.LookAt(con);
			cP.transform.position = con.position;
			cP.transform.LookAt(t.transform);
		}

		private void DrawDebugLines()
		{
			if (c.Controller.debugActive == false) return;
			var position = oP.transform.position;
			var position1 = oO.transform.position;
			var position2 = cO.transform.position;
			var position3 = cP.transform.position;
			Debug.DrawLine(position3, position2, Color.red);
			Debug.DrawLine(position, position1, Color.red);
			Debug.DrawLine(position, position3, Color.blue);
			Debug.DrawLine(position1, position2, Color.blue);
			var position4 = tS.transform.position;
			Debug.DrawLine(position, position4, Color.green);
			Debug.DrawLine(position3, position4, Color.green);
			Debug.DrawLine(t.transform.position, position4, Color.yellow);
		}
	}
}