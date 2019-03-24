using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(Rigidbody))]
	public class SelectableObject : MonoBehaviour
	{ 
		#region Inspector and Variables
		private ObjectSelection c;
		private Manipulation f;
		private Rotation r;
		
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;
		private Vector3 defaultLocalScale;

		private bool active;
		
		private RotationLock rotLock;
		private float gazeAngle;
		private float manualRef;
		private Rigidbody rb;
		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public Renderer Renderer { get; private set; }

		public enum RotationLock
		{
			FreeRotation
		}
		private enum ButtonTrigger
		{
			OnButtonDown,
			OnButtonUp				
		}
		
	
		[BoxGroup("Script Setup")] [SerializeField] [Required] private GameObject player;
		[BoxGroup("Script Setup")] [HideIf("button")] [SerializeField] private bool grab;
		[BoxGroup("Script Setup")] [HideIf("grab")] [SerializeField] private bool button;
		[BoxGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent] private bool startsActive;
		[BoxGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent] private bool menu;
		[BoxGroup("Script Setup")] [ShowIf("button")] [ShowIf("menu")] [Indent(2)] public GameObject menuItems;
		[BoxGroup("Script Setup")] public bool toolTip;
		[BoxGroup("Script Setup")] [ShowIf("toolTip")] [Indent] public string toolTipText;
		
		[TabGroup("Manipulation Settings")] [HideIf("button")] [Range(0, 1f)] public float moveForce = .15f;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [Range(0, 10f)] public float latency = 4.5f;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [SerializeField] private bool gravity;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [ShowIf("grab")] [Space(3)] public bool directGrab = true;
		[TabGroup("Manipulation Settings")] [HideIf("button")] [ShowIf("grab")] [ShowIf("directGrab")] [SerializeField] [Indent] [Range(.1f, 5f)] private float directGrabDistance = .15f;
		[TabGroup("Rotation Settings")] [HideIf("button")] [SerializeField] public bool freeRotationEnabled;
		[TabGroup("Rotation Settings")] [HideIf("button")] [HideIf("freeRotationEnabled")] [Indent] [SerializeField] public RotationLock rotationLock;
		
		[TabGroup("Button Settings")] [ShowIf("button")] public TextMeshPro buttonText;
		[TabGroup("Button Settings")] [ShowIf("button")] public Renderer buttonBack;
		[TabGroup("Button Settings")] [ShowIf("button")] [Space(10)] [SerializeField] private bool genericSelectState;
		[TabGroup("Button Settings")] [ShowIf("button")] [ShowIf("genericSelectState")] [Indent] [Range(0, 1f)] [SerializeField] private float selectOffset;
		[TabGroup("Button Settings")] [ShowIf("button")] [ShowIf("genericSelectState")] [Indent] [Range(0, 1f)] [SerializeField] private float selectScale;
		[TabGroup("Button Settings")] [ShowIf("button")] [ShowIf("genericSelectState")] [Indent] [SerializeField] private TMP_FontAsset activeFont;
		[TabGroup("Button Settings")] [ShowIf("button")] [ShowIf("genericSelectState")] [Indent] [SerializeField] private TMP_FontAsset inactiveFont;
		[TabGroup("Button Settings")] [ShowIf("button")] [ShowIf("genericSelectState")] [Indent] [SerializeField] private Color activeColor = new Color(0,0,0,255);
		[TabGroup("Button Settings")] [ShowIf("button")] [ShowIf("genericSelectState")] [Indent] [SerializeField] private Color inactiveColor = new Color(0,0,0,255);
		[TabGroup("Button Settings")] [ShowIf("button")] [Space(5)] private ButtonTrigger buttonTrigger;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] [Space(10)] private UnityEvent selectStart;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] private UnityEvent selectStay;
		[TabGroup("Button Settings")] [ShowIf("button")] [SerializeField] private UnityEvent selectEnd;

		[BoxGroup("Visual Settings")] [SerializeField] private bool reactiveMat;
		[BoxGroup("Visual Settings")] [ShowIf("reactiveMat")] [SerializeField] [Indent] [Range(0, 1f)] private float clippingDistance;

		[BoxGroup("Hover Settings")] [SerializeField] private bool hover;
		[BoxGroup("Hover Settings")] [ShowIf("hover")] [Indent] [SerializeField] private bool genericHoverEffect;
		[BoxGroup("Hover Settings")] [ShowIf("hover")] [ShowIf("genericHoverEffect")] [Indent(2)] [Range(0, 1f)] [SerializeField] private float hoverOffset;
		[BoxGroup("Hover Settings")] [ShowIf("hover")] [ShowIf("genericHoverEffect")] [Indent(2)] [Range(0, 1f)] [SerializeField] private float hoverScale;
		[BoxGroup("Hover Settings")] [ShowIf("hover")] [HideIf("genericHoverEffect")] [SerializeField] private UnityEvent hoverStart;
		[BoxGroup("Hover Settings")] [ShowIf("hover")] [HideIf("genericHoverEffect")] [SerializeField] private UnityEvent hoverStay;
		[BoxGroup("Hover Settings")] [ShowIf("hover")] [HideIf("genericHoverEffect")] [SerializeField] private UnityEvent hoverEnd;
		
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
			
			if(!button) return;
			ToggleState(startsActive);
			active = startsActive;
		}
		private void AssignComponents()
		{
			c = player.GetComponent<ObjectSelection>();
			f = player.GetComponent<Manipulation>();
			r = player.GetComponent<Rotation>();
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
			ManageList(o, c.lHandList, CheckHand(o, c.gazeList, c.manual, AngleL,f.disableRightGrab, button), c.disableLeftHand, WithinRange(c.setSelectionRange, transform, c.Controller.LeftControllerTransform(), c.selectionRange));
			ManageList(o, c.rHandList, CheckHand(o, c.gazeList, c.manual, AngleR,f.disableLeftGrab, button), c.disableRightHand, WithinRange(c.setSelectionRange, transform, c.Controller.RightControllerTransform(), c.selectionRange));
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

		private void ToggleState(bool a)
		{
			switch (a)
			{
				case true:
					Set.VisualState(transform, this, Set.LocalScale(defaultLocalScale, selectScale), Set.LocalPosition(defaultLocalPosition, selectOffset), activeFont, activeColor);
					break;
				case false:
					Set.VisualState(transform, this, defaultLocalScale, defaultLocalPosition, inactiveFont, inactiveColor);
					break;
				default:
					throw new ArgumentException();
			}
		}
		
		public void GrabStart(Transform con)
		{
			if (!grab) return;
			Set.RigidBody(rb, moveForce, latency,false, gravity);
			f.OnStart(con);
		}
		public void GrabStay(Transform con, Transform mid, Transform end)
		{
			if (!grab) return;
			
			f.OnStay(con, mid, end, transform, c.lineRenderQuality);
			
			switch (f.manipulationType)
			{
				case Manipulation.ManipulationType.Lerp:
					if (DualGrab(c.Controller.LeftGrab(), c.Controller.RightGrab(), c.lSelectableObject, c.rSelectableObject, this))
					{
						Set.TransformLerpPosition(transform, f.mP.transform, .1f);
						break;
					}
					if (c.Controller.RightGrab() && c.rSelectableObject == this)
					{
						Set.TransformLerpPosition(transform, f.tSr.transform, .1f);
						break;
					}
					if (c.Controller.LeftGrab() && c.lSelectableObject == this)
					{
						Set.TransformLerpPosition(transform, f.tSl.transform, .1f);
					}
					break;
				case Manipulation.ManipulationType.Physics:
					if (DualGrab(c.Controller.LeftGrab(), c.Controller.RightGrab(), c.lSelectableObject, c.rSelectableObject, this))
					{
						Set.AddForcePosition(rb, transform, f.mP.transform, c.Controller.debugActive);
						Set.AddForceRotation(rb, transform, f.mP.transform, 100f);
						break;
					}
					if (c.Controller.RightGrab() && c.rSelectableObject == this)
					{
						Set.AddForcePosition(rb, transform, f.tSr.transform, c.Controller.debugActive);
						break;
					}
					if (c.Controller.LeftGrab() && c.lSelectableObject == this)
					{
						Set.AddForcePosition(rb, transform, f.tSl.transform, c.Controller.debugActive);
					}
					break;
				default:
					throw new ArgumentException();
			}
		}
		private static bool DualGrab(bool l, bool r, Object lS, Object rS, Object s)
		{
			return l && r && lS == s && rS == s;
		}
		public void GrabEnd(Transform con)
		{
			if (!grab) return;
			c.gazeList.Clear();
			
			Set.RigidBody(rb, moveForce, latency,false, gravity);
			
			f.OnEnd(con);
		}
		public void HoverStart()
		{
			hoverStart.Invoke();

			if (!genericHoverEffect || !hover) return;
			
			var t = transform;
			defaultLocalScale = t.localScale;
			defaultLocalPosition = t.localPosition;
			
			// BUG: Replace with Tween Sequence!

			t.localScale = Set.LocalScale(defaultLocalScale, hoverScale);
			
			if (rb.velocity != Vector3.zero) return;
			t.localPosition = Set.LocalPosition(defaultLocalPosition, hoverOffset);
		}
		public void HoverStay()
		{
			hoverStay.Invoke();
		}
		public void HoverEnd()
		{
			hoverEnd.Invoke();
			
			if (!genericHoverEffect || !hover) return;
			
			var t = transform;
			
			// BUG: Replace with Tween Sequence!
			
			t.localScale = defaultLocalScale;
			
			if (rb.velocity != Vector3.zero) return;
			t.localPosition = defaultLocalPosition;
		}
		public void SelectStart()
		{
			selectStart.Invoke();

			if (!genericSelectState || !button) return;
			switch (buttonTrigger)
			{
				case ButtonTrigger.OnButtonDown:
					active = !active;
					ToggleState(active);
					break;
				case ButtonTrigger.OnButtonUp:
					break;
				default:
					throw new ArgumentException();
			}
		}
		public void SelectStay()
		{
			selectStay.Invoke();
		}
		public void SelectEnd()
		{
			selectEnd.Invoke();
			
			if (!genericSelectState || !button) return;
			switch (buttonTrigger)
			{
				case ButtonTrigger.OnButtonDown:
					break;
				case ButtonTrigger.OnButtonUp:
					active = !active;
					ToggleState(active);
					break;
				default:
					throw new ArgumentException();
			}
		}
	}
}
