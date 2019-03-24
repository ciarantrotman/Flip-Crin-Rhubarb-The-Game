using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
		public Renderer Renderer { get; private set; } 
	
		[BoxGroup("Script Setup")] [SerializeField] [Required] private GameObject player;
		[BoxGroup("Script Setup")] [HideIf("button")] [SerializeField] private bool grab;
		[BoxGroup("Script Setup")] [HideIf("grab")] [SerializeField] private bool button;
		[BoxGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent] private bool menu;
		[BoxGroup("Script Setup")] [ShowIf("button")] [ShowIf("menu")] [Indent(2)] public GameObject menuItems;
		[BoxGroup("Script Setup")] public bool toolTip;
		[BoxGroup("Script Setup")] [ShowIf("toolTip")] [Indent] public string toolTipText;
		
		[TabGroup("Manipulation Settings")] [HideIf("button")] public float moveSpeed = 1f;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [SerializeField] private bool gravity;
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
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] [Space(10)] private UnityEvent selectStart;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] private UnityEvent selectStay;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] private UnityEvent selectEnd;

		[BoxGroup("Visual Settings")] [SerializeField] private bool reactiveMat;
		[BoxGroup("Visual Settings")] [ShowIf("reactiveMat")] [SerializeField] [Indent] private float clippingDistance;
		[Header("Hover Events")]
		[FoldoutGroup("Hover Events")] [SerializeField] private UnityEvent hoverStart;
		[FoldoutGroup("Hover Events")] [SerializeField] private UnityEvent hoverStay;
		[FoldoutGroup("Hover Events")] [SerializeField] private UnityEvent hoverEnd;
		
		private static readonly int Threshold = Shader.PropertyToID("_ClipThreshold");
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
			AssignComponents();
			SetupRigidBody();
			SetupManipulation();
			ToggleList(gameObject, c.gazeList);
		}
		private void AssignComponents()
		{
			c = player.GetComponent<ObjectSelection>();
			f = player.GetComponent<Manipulation>();
			r = player.GetComponent<FreeRotation>();
			Renderer = GetComponent<Renderer>();
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
			GetAngles();
			CheckDirectGrab();
			ReactiveMaterial();

			var o = gameObject;
			CheckGaze(o, gazeAngle, c.gaze, c.gazeList, c.lHandList, c.rHandList, c.globalList);
			ManageList(o, c.lHandList, CheckHand(o, c.gazeList, c.manual, AngleL,f.lHandDisable, button), c.disableLeftHand, WithinRange(c.setSelectionRange, transform, c.Controller.LeftControllerTransform(), c.selectionRange));
			ManageList(o, c.rHandList, CheckHand(o, c.gazeList, c.manual, AngleR,f.rHandDisable, button), c.disableRightHand, WithinRange(c.setSelectionRange, transform, c.Controller.RightControllerTransform(), c.selectionRange));
		}

		private void ReactiveMaterial()
		{
			if (!reactiveMat) return;
			
			Renderer.material.SetFloat(Threshold, clippingDistance);
			Set.ReactiveMaterial(Renderer, c.Controller.LeftControllerTransform(), c.Controller.RightControllerTransform());
		}
		
		private void GetAngles()
		{
			var position = transform.position;
			gazeAngle = Vector3.Angle(position - c.Controller.CameraPosition(), c.Controller.CameraForwardVector());
			AngleL = Vector3.Angle(position - c.Controller.LeftControllerTransform().position, c.Controller.LeftForwardVector());
			AngleR = Vector3.Angle(position - c.Controller.RightControllerTransform().position, c.Controller.RightForwardVector());
		}
		private static void CheckGaze(GameObject o, float a, float c, List<GameObject> g, List<GameObject> l, List<GameObject> r, List<GameObject> global)
		{
			if (!global.Contains(o))
			{
				g.Remove(o);
				l.Remove(o);
				r.Remove(o);
			}
				
			if (a < c/2 && !g.Contains(o))
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
		private static bool CheckHand(GameObject g, List<GameObject> gaze, float m, float c, bool b, bool button)
		{
			if (b && !button) return false;
			if (!gaze.Contains(g)) return false;
			return m > c / 2;
		}

		private static bool WithinRange(bool enabled, Transform self, Transform user, float range)
		{
			if (!enabled) return true;
			return Vector3.Distance(self.position, user.position) <= range;
		}

		private static void ManageList(GameObject g, List<GameObject> l, bool b, bool d, bool r)
		{
			if (d || !r) return;
			
			if (b && !l.Contains(g))
			{
				l.Add(g);
			}
			else if (!b && l.Contains(g))
			{
				l.Remove(g);
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
		
		public void HoverStart()
		{
			
		}

		public void HoverStay()
		{
			
		}
		public void HoverEnd()
		{

		}
		public void SelectStart()
		{
			
		}
		public void SelectStay()
		{
			
		}
		public void SelectEnd()
		{
			
		}
	}
}
