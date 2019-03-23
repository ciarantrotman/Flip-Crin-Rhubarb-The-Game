using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(Rigidbody))]
	public class SelectableObject : MonoBehaviour
	{ 
		#region Inspector and Variables
		private ObjectSelection c;
		private Manipulation f;
		private FreeRotation r;
		
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;
		private RotationLock rotLock;
		private GameObject instantiatedAxes;
		private float gazeAngle;
		private float manualRef;
		private Rigidbody rb;
		private bool _throw;
		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		
		[TabGroup("Script Setup")] [SerializeField] private GameObject player;
		[TabGroup("Script Setup")] [HideIf("button")] [SerializeField] private bool grab;
		[TabGroup("Script Setup")] [HideIf("grab")] [SerializeField] private bool button;
		[TabGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent] private bool menu;
		[TabGroup("Script Setup")] [ShowIf("button")] [ShowIf("menu")] [Indent(2)] public GameObject menuItems;
		[TabGroup("Script Setup")] public bool toolTip;
		[TabGroup("Script Setup")] [ShowIf("toolTip")] [Indent] public string toolTipText;
		
		[TabGroup("Object Behaviour")] [HideIf("button")] public float moveSpeed = 1f;
		[TabGroup("Object Behaviour")] [HideIf("button")] private bool gravity;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [ShowIf("grab")] [Space(3)] public bool directGrab = true;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [ShowIf("grab")] [ShowIf("directGrab")] [SerializeField] [Indent] [Range(.1f, 5f)] private float directGrabDistance = .15f;
		public enum RotationLock
		{
			FreeRotation
		}
		[TabGroup("Rotation Settings")] [HideIf("button")] [SerializeField] public bool freeRotationEnabled;
		[TabGroup("Rotation Settings")] [HideIf("button")] [HideIf("freeRotationEnabled")] [Indent] [SerializeField] public RotationLock rotationLock;
		
		[TabGroup("Button Settings")] [ShowIf("button")] public TextMeshPro buttonText;
		[TabGroup("Button Settings")] [ShowIf("button")] public Renderer buttonBack;
		[TabGroup("Button Settings")] [ShowIf("button")] [Space(10)] [SerializeField] private UnityEvent @select;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] private UnityEvent hover;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] private UnityEvent hoverEnd;
		#endregion
		private void Start ()
		{
			InitialiseSelectableObject();
		}
		private void OnEnable()
		{
			InitialiseSelectableObject();
		}
		private void OnDisable()
		{
			var g = gameObject;
			ToggleList(g, c.gazeList);
			ToggleList(g, c.lHandList);
			ToggleList(g, c.rHandList);
		}
		private void InitialiseSelectableObject()
		{
			//CheckPlayer();
			AssignComponents();
			SetupRigidBody();
			SetupManipulation();
			ToggleList(gameObject, c.gazeList);
		}
		private void CheckPlayer()
		{
			if (player != null && player.GetComponent<ObjectSelection>() != null &&
			    player.GetComponent<Manipulation>() != null) return;
			Debug.Log("Make sure the right scripts are attached to the VR Player");
			GetComponent<SelectableObject>().enabled = false;
			Destroy(this);
		}
		private void AssignComponents()
		{
			c = player.GetComponent<ObjectSelection>();
			f = player.GetComponent<Manipulation>();
			r = player.GetComponent<FreeRotation>();
		}
		private void SetupRigidBody()
		{
			rb = GetComponent<Rigidbody>();
			rb.freezeRotation = true;
			rb.useGravity = !button && gravity;
		}
		private void SetupManipulation()
		{
			if (freeRotationEnabled)
			{
				rotationLock = RotationLock.FreeRotation;
			}
		}
		private static void ToggleList(GameObject g, List<GameObject> l)
		{
			if (l.Contains(g))
			{
				l.Remove(g);
			}
			else if (!l.Contains(g))
			{
				l.Add(g);
			}
		}
		private void Update()
		{
			SelectionRange();
			GetAngles();
			CheckDirectGrab();

			var o = gameObject;
			CheckGaze(o, gazeAngle, c.gaze, c.gazeList, c.lHandList, c.rHandList);
			ManageList(o, c.lHandList, CheckHand(o, c.gazeList, c.lHandList, c.manual, AngleL,f.lHandDisable, button), c.disableLeftHand);
			ManageList(o, c.rHandList, CheckHand(o, c.gazeList, c.rHandList, c.manual, AngleR,f.rHandDisable, button), c.disableRightHand);
			
			OnHoverEnd();
		}
		private void GetAngles()
		{
			var position = transform.position;
			gazeAngle = Vector3.Angle(position - c.Controller.CameraPosition(), c.Controller.CameraForwardVector());
			AngleL = Vector3.Angle(position - c.Controller.LeftControllerTransform().position, c.Controller.LeftForwardVector());
			AngleR = Vector3.Angle(position - c.Controller.RightControllerTransform().position, c.Controller.RightForwardVector());
		}
		private static void CheckGaze(GameObject o, float a, float c, List<GameObject> g, List<GameObject> l, List<GameObject> r)
		{
			if (a < c/2 && g.Contains(o) == false)
			{
				g.Add(o);
			}
			else if (a > c/2)
			{
				g.Remove(o);
				l.Remove(o);
				r.Remove(o);
			}
		}
		
		private static bool CheckHand(GameObject g, List<GameObject> gaze, List<GameObject> l, float m, float c, bool b, bool button)
		{
			if (b && !button) return false;
			if (!gaze.Contains(g)) return false;
			return m > c / 2;
		}

		private static void ManageList(GameObject g, List<GameObject> l, bool b, bool d)
		{
			if (d) return;
			
			if (b && !l.Contains(g))
			{
				l.Add(g);
			}
			else if (!b && l.Contains(g))
			{
				l.Remove(g);
			}
		}
		
		public void OnHover()
		{
			hover.Invoke();
		}
		private void OnHoverEnd()
		{
			if(c.rFocusObject == gameObject || c.lFocusObject == gameObject) return;
			hoverEnd.Invoke();
		}
		public void OnSelect()
		{
			select.Invoke();
		}
		private void SelectionRange()
		{
			if (!c.setSelectionRange) return;
		
			if (Vector3.Distance(transform.position, player.transform.position) >= c.selectionRange)
			{
				if (!c.globalList.Contains(gameObject)) return;
				
				c.globalList.Remove(gameObject);
				c.rHandList.Remove(gameObject);
				c.lHandList.Remove(gameObject);
				c.gazeList.Remove(gameObject);
				c.lFocusObject = null;
				c.rFocusObject = null;
				c.lSelectableObject = null;
				c.rSelectableObject = null;
			}
			else if (Vector3.Distance(transform.position, player.transform.position) < c.selectionRange)
			{
				if (!c.globalList.Contains(gameObject))
				{
					c.globalList.Add(gameObject);
				}
			}
		}
		private void CheckDirectGrab()
		{
			if (Vector3.Distance(transform.position, c.Controller.LeftControllerTransform().position) <= directGrabDistance)
			{
				c.grabObject = gameObject;
			}
		}
		
		public void GrabStart(Transform con)
		{
			if (!grab) return;
			var o = gameObject;
			c.grabObject = o;
			c.disableSelection = true;
			Set.RigidBody(rb, moveSpeed, true, false);
			f.OnStart(con);
		}
		public void GrabStay(Transform con, Transform mid)
		{
			if (!grab) return;
			
			f.OnStay(con, mid, transform, c.quality);
			switch (f.manipulationType)
			{
				case Manipulation.ManipulationType.Lerp:
					Set.LerpPosition(transform, f.tS.transform, .1f);
					break;
				case Manipulation.ManipulationType.Physics:
					Set.AddForcePosition(rb, transform, f.tS.transform, c.Controller.debugActive);
					break;
				default:
					throw new ArgumentException();
			}
		}
		public void GrabEnd(Transform con)
		{
			if (!grab) return;
			c.disableSelection = false;
			c.gazeList.Clear();
			c.LStay = false;
			c.RStay = false;
			c.grabObject = null;
			Set.RigidBody(rb, moveSpeed, false, gravity);
			
			f.OnEnd();
		}
	}
}
