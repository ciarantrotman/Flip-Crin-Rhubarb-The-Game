using System;
using System.Xml.Serialization;
using Sirenix.OdinInspector;
using UnityEditor.U2D;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VR_Prototyping.Scripts
{
	[RequireComponent(typeof(ObjectSelection))]
	public class Manipulation : MonoBehaviour
	{
		#region Inspector and Variables
		private GameObject player;
		
		private ObjectSelection c;
		
		private GameObject fM;
		[HideInInspector] public GameObject mP;
		
		private GameObject cPr;
		private GameObject oPr;	
		private GameObject cOr;
		private GameObject oOr;
		private GameObject tr;
		private GameObject cFr;
		[HideInInspector] public GameObject tSr;

		private GameObject cPl;
		private GameObject oPl;	
		private GameObject cOl;
		private GameObject oOl;
		private GameObject tl;
		private GameObject cFl;
		[HideInInspector] public GameObject tSl;
		
		private float initialDistance;
		private float m;
		private float z;
				
		public enum ManipulationType
		{
			Physics,
			Lerp		
		}
		[TabGroup("Grab Settings")] public ManipulationType manipulationType;
		[TabGroup("Grab Settings")] [SerializeField] private bool objectMovesWithYouTeleport;
		[TabGroup("Grab Settings")] public bool disableLeftGrab;
		[TabGroup("Grab Settings")] public bool disableRightGrab;
		[TabGroup("Rotation Settings")] public bool enableRotation;
		[TabGroup("Rotation Settings")] [ShowIf("enableRotation")] [Indent] [Range(0f, 10f)] public float force;
		
		[BoxGroup("Snapping")] [SerializeField] private bool distanceSnapping = true;
		[BoxGroup("Snapping")] [ShowIf("distanceSnapping")] [Indent] [Range(0f, 5f)] [SerializeField] private float snapDistance = 1f;
		[BoxGroup("Snapping")] [ShowIf("distanceSnapping")] [Indent] [SerializeField] private bool maximumDistance;
		
		#endregion
		private void Start () 
		{
			SetupGameObjects();
			
			c = GetComponent<ObjectSelection>();
			player = c.gameObject;
			
			if (objectMovesWithYouTeleport)
			{
				fM.transform.parent = player.transform;
			}	
		}

		private void SetupGameObjects()
		{
			fM = new GameObject("Manipulation/Manipulation");
			
			mP = new GameObject("Manipulation/MidPoint");
			mP.transform.SetParent(fM.transform);
			
			tr = new GameObject("Manipulation/Target/Right");
			tr.transform.SetParent(fM.transform);
			
			cFr = new GameObject("Manipulation/Controller/Follow/Right");
			cFr.transform.SetParent(fM.transform);
			
			cOr = new GameObject("Manipulation/Controller/Original/Right");
			cOr.transform.SetParent(cFr.transform);
			
			cPr = new GameObject("Manipulation/Controller/Proxy/Right");
			cPr.transform.SetParent(cFr.transform);
			
			tSr = new GameObject("Manipulation/Target/Scaled/Right");
			tSr.transform.SetParent(cPr.transform);
			
			oPr = new GameObject("Manipulation/Object/Proxy/Right");
			oPr.transform.SetParent(cPr.transform);
			
			oOr = new GameObject("Manipulation/Object/Original/Right");
			oOr.transform.SetParent(cPr.transform);
			
			tl = new GameObject("Manipulation/Target/Left");
			tl.transform.SetParent(fM.transform);
			
			cFl = new GameObject("Manipulation/Controller/Follow/Left");
			cFl.transform.SetParent(fM.transform);
			
			cOl = new GameObject("Manipulation/Controller/Original/Left");
			cOl.transform.SetParent(cFl.transform);
			
			cPl = new GameObject("Manipulation/Controller/Proxy/Left");
			cPl.transform.SetParent(cFl.transform);
			
			tSl = new GameObject("Manipulation/Target/Scaled/Left");
			tSl.transform.SetParent(cPl.transform);
			
			oPl = new GameObject("Manipulation/Object/Proxy/Left");
			oPl.transform.SetParent(cPl.transform);
			
			oOl = new GameObject("Manipulation/Object/Original/Left");
			oOl.transform.SetParent(cPl.transform);
		}
		private void Update()
		{				
			Set.SplitPosition(c.Controller.CameraTransform(), c.Controller.LeftControllerTransform(), cFl.transform);
			Set.SplitPosition(c.Controller.CameraTransform(), c.Controller.RightControllerTransform(), cFr.transform);
			Set.MidpointPosition(mP.transform, tSl.transform, tSr.transform, true);
			
			FollowFocusObjects();
		}

		private void FollowFocusObjects()
		{
			if (c.lFocusObject != null)
			{
				ObjectMethods.FocusObjectFollow(c.lFocusObject.transform, c.Controller.LeftControllerTransform(), tl.transform, tSl.transform, oOl.transform, cOl.transform, oPl.transform, c.Controller.LeftGrab());
			}

			if (c.rFocusObject != null)
			{
				ObjectMethods.FocusObjectFollow(c.rFocusObject.transform, c.Controller.RightControllerTransform(), tr.transform, tSr.transform, oOr.transform, cOr.transform, oPr.transform, c.Controller.RightGrab());
			}
		}
		public void OnStart(Transform con)
		{
			switch (con == c.Controller.LeftControllerTransform())
			{
				case true:
					ObjectMethods.GrabStart(cFl, cPl, tl, cOl, con);
					Set.Transforms(tl.transform, c.lFocusObject.transform);
					Set.Transforms(tSl.transform, tl.transform);
					Set.Position(oPl.transform, c.lFocusObject.transform);
					Set.Position(oOl.transform, c.lFocusObject.transform);
					break;
				case false:
					ObjectMethods.GrabStart(cFr, cPr, tr, cOr, con);
					Set.Transforms(tr.transform, c.rFocusObject.transform);
					Set.Transforms(tSr.transform, tr.transform);
					Set.Position(oPr.transform, c.rFocusObject.transform);
					Set.Position(oOr.transform, c.rFocusObject.transform);
					break;
				default:
					throw new ArgumentException();
			}
		}

		public void OnStay(Transform con, Transform mid, Transform end, Transform grabObject, int q)
		{
			switch (con == c.Controller.LeftControllerTransform())
			{
				case true:
					ControllerFollowing(con, cFl, cPl, tl);
					tSl.transform.localPosition = new Vector3(0, 0, MagnifiedDepth(cPl, cOl, oOl, tSl, snapDistance, c.selectionRange - c.selectionRange * .25f, maximumDistance));
					break;
				case false:
					ControllerFollowing(con, cFr, cPr, tr);
					tSr.transform.localPosition = new Vector3(0, 0, MagnifiedDepth(cPr, cOr, oOr, tSr, snapDistance, c.selectionRange - c.selectionRange * .25f, maximumDistance));
					break;
				default:
					throw new ArgumentException();
			}
		}

		private static float MagnifiedDepth(GameObject conP, GameObject conO, GameObject objO, GameObject objP, float snapDistance, float max, bool limit)
		{
			var depth = conP.transform.localPosition.z / conO.transform.localPosition.z;
			var distance = Vector3.Distance(objO.transform.position, objP.transform.position);
				
			if (distance >= max && limit) return max;
			if (distance < snapDistance) return objO.transform.localPosition.z * Mathf.Pow(depth, 2);														
			return objO.transform.localPosition.z * Mathf.Pow(depth, 2.5f);
		}
		
		public void OnEnd(Transform con)
		{
			switch (con == c.Controller.LeftControllerTransform())
			{
				case true:
					tl.transform.SetParent(null);
					break;
				case false:
					tr.transform.SetParent(null);
					break;
				default:
					throw new ArgumentException();
			}
		}
		
		private void ControllerFollowing(Transform con, GameObject f, GameObject p, GameObject target)
		{
			f.transform.LookAt(con);
			p.transform.position = con.position;
			p.transform.LookAt(target.transform);
		}
	}
}