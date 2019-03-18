using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(ControllerTransforms))]
	public class IndirectObjectSelection : MonoBehaviour
	{
		#region Inspector and Variables
		public ControllerTransforms Controller { get; private set; }
		public bool RightManipulationStay { get; set; }
		public bool LeftManipulationStay { get; set; }
		public enum ControllerEnum
		{
			Left,
			Right
		}
		private GameObject lLineRendererTarget;
		private GameObject rLineRendererTarget;
		private SelectableObject rightSelectableObject;
		private SelectableObject leftSelectableObject;
		private bool leftSelectPrevious;
		private bool rightSelectPrevious;
		private int lCount;
		private int rCount;
		[HideInInspector] public ControllerEnum controllerEnum;
		[HideInInspector] public GameObject rightGrabObject;
		[HideInInspector] public GameObject leftGrabObject;
		[HideInInspector] public GameObject activeObject;
		[HideInInspector] public GameObject lMidPoint;
		[HideInInspector] public GameObject rMidPoint;
		[HideInInspector] public GameObject leftFocusObject;
		[HideInInspector] public GameObject rightFocusObject;
		[HideInInspector] public LineRenderer leftLr;
		[HideInInspector] public LineRenderer rightLr;
		
		[Header("Define Selection Angles")]
		[TabGroup("Selection Settings")] [SerializeField] [Range(0f, 180f)] public float gazeFrustumAngle = 60f;		// angle from the HMD to select objects
		[TabGroup("Selection Settings")] [SerializeField] [Range(0f, 180f)] public float manualFrustumAngle = 25f;		// angle from the controller to select objects
		[TabGroup("Selection Settings")] [SerializeField] public bool setSelectionRange;		
		[TabGroup("Selection Settings")] [SerializeField] [ShowIf("setSelectionRange")] [Indent] [Range(0f, 250f)] public float selectionRange = 25f;		
		[TabGroup("Object Lists")] public List<GameObject> allSelectableObjects;
		[TabGroup("Object Lists")] public List<GameObject> objectsInGaze;
		[TabGroup("Object Lists")] public List<GameObject> objectsInManualRight;
		[TabGroup("Object Lists")] public List<GameObject> objectsInManualLeft;
		[Header("References to GameObjects")]
		[TabGroup("Aesthetics and References")] [SerializeField] private bool curvedLineRender;
		[Space(5)][TabGroup("Aesthetics and References")] [ShowIf("curvedLineRender")] [Indent] [SerializeField] public int lineSteps = 15;
		[Space(10)] [TabGroup("Aesthetics and References")] [SerializeField] public TextMeshPro leftHandText;
		[TabGroup("Aesthetics and References")] [SerializeField] public TextMeshPro rightHandText;
		#endregion
		private void Start ()
		{
			Controller = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			
			leftLr = Controller.LeftControllerTransform().gameObject.AddComponent<LineRenderer>();
			rightLr = Controller.RightControllerTransform().gameObject.AddComponent<LineRenderer>();
			
			Setup.LineRender(leftLr, Controller.lineRenderMat, .005f, false);
			Setup.LineRender(rightLr, Controller.lineRenderMat, .005f, false);
			
			SetupMidpoints(lMidPoint.transform, Controller.LeftControllerTransform());
			SetupMidpoints(rMidPoint.transform, Controller.RightControllerTransform());
			
			leftHandText.renderer.enabled = false;
			rightHandText.renderer.enabled = false;
		}

		private void SetupGameObjects()
		{
			lMidPoint = new GameObject("LeftMidPoint");
			rMidPoint = new GameObject("RightMidPoint");
			lLineRendererTarget = new GameObject("LeftLR");
			rLineRendererTarget = new GameObject("RightLR");
		}
		private void Update () 
		{		
			FocusObject(LeftManipulationStay, objectsInManualLeft, leftFocusObject, lLineRendererTarget, false);
			FocusObject(RightManipulationStay, objectsInManualRight, rightFocusObject, rLineRendererTarget, true);
		
			DrawLineRenderer();
			DrawDebugLines();
		}
		private void FixedUpdate()
		{
			Manipulation(leftFocusObject, leftGrabObject,leftSelectableObject, Controller.LeftGrab(), lCount, false);
			Manipulation(rightFocusObject, rightGrabObject,rightSelectableObject, Controller.RightGrab(), rCount, true);
		}

		private void LateUpdate()
		{
			//Selection(leftFocusObject, leftSelectableObject, leftSelect, leftSelectPrevious);
			//Selection(rightFocusObject, rightSelectableObject, rightSelect, rightSelectPrevious);
		
			//leftSelectPrevious = leftSelect;
			//rightSelectPrevious = rightSelect;
		}

		private void Manipulation(GameObject focusObject, GameObject grabObject, SelectableObject selectableObject, bool grip, int grabCount, bool right)
		{
			if (focusObject == null || selectableObject == null) return;
			if (grip)
			{
				grabObject = focusObject;
				controllerEnum = right? ControllerEnum.Right : ControllerEnum.Left;
				selectableObject.IndirectManipulationStart();
				selectableObject.IndirectManipulationStay();
				grabCount++; // only call IndirectManipulationStart once
			}
			else if (!grip && grabCount > 0)
			{
				selectableObject.IndirectManipulationEnd();
				grabCount = 0;
			}
		}
	
		private static void Selection(Object focusObject, SelectableObject button, bool c, bool p)
		{
			if (focusObject == null || button == null) return;
			if (c != p && c)
			{
				button.OnSelect();
			}
		}

		#region Find the Active Objects
	
		private void FocusObject(bool stay, List<GameObject> objects, GameObject focusObject, GameObject target, bool right)
		{
			if (stay) return;
			switch (right)
			{
				case true:
					objects.Sort(GetRightActiveObject);
					leftFocusObject = objects.Count > 0 ? objects[0].gameObject : null;
					if (leftFocusObject == null || focusObject.GetComponent<SelectableObject>() == null) return;
					leftSelectableObject = focusObject.GetComponent<SelectableObject>();
					leftSelectableObject.OnHover();
					target.transform.position = Vector3.Lerp(target.transform.position, leftFocusObject.transform.position, .25f);
					break;
				case false:
					objects.Sort(GetLeftActiveObject);
					rightFocusObject = objects.Count > 0 ? objects[0].gameObject : null;
					if (rightFocusObject == null || focusObject.GetComponent<SelectableObject>() == null) return;
					rightSelectableObject = focusObject.GetComponent<SelectableObject>();
					rightSelectableObject.OnHover();
					target.transform.position = Vector3.Lerp(target.transform.position, rightFocusObject.transform.position, .25f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	
		private static int GetLeftActiveObject(GameObject obj1, GameObject obj2)
		{
			return obj1.GetComponent<SelectableObject>().manualLeftAngle.CompareTo(obj2.GetComponent<SelectableObject>().manualLeftAngle);
		}
		private static int GetRightActiveObject(GameObject obj1, GameObject obj2)
		{
			return obj1.GetComponent<SelectableObject>().manualRightAngle.CompareTo(obj2.GetComponent<SelectableObject>().manualRightAngle);
		}
		#endregion
		#region Linerenderers Setup and Drawing
		private static void SetupMidpoints(Transform m, Transform p)
		{
			m.position = p.position;
			m.parent = p;
			m.localRotation = new Quaternion(0,0,0,0);
		}
		private void DrawLineRenderer()
		{
			if (leftFocusObject != null)
			{
				leftLr.enabled = true;
			
				float depth = Vector3.Distance(Controller.LeftControllerTransform().position, lLineRendererTarget.transform.position) / 2;
				lMidPoint.transform.localPosition = new Vector3(0, 0, depth);
				switch (curvedLineRender)
				{
					case true:
						BezierCurve.BezierLineRenderer(leftLr, 
							Controller.LeftControllerTransform().position,
							lMidPoint.transform.position, 
							lLineRendererTarget.transform.position,
							lineSteps);
						break;
					case false:
						leftLr.SetPosition(0, Controller.LeftControllerTransform().position);
						leftLr.SetPosition(1, lLineRendererTarget.transform.position);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			if (leftFocusObject == null || RightManipulationStay || LeftManipulationStay)
			{
				leftLr.enabled = false;
			}
			if (rightFocusObject != null)
			{
				rightLr.enabled = true;

				float depth = Vector3.Distance(Controller.RightControllerTransform().position, rLineRendererTarget.transform.position) / 2;
				rMidPoint.transform.localPosition = new Vector3(0, 0, depth);
				switch (curvedLineRender)
				{
					case true:
						BezierCurve.BezierLineRenderer(rightLr, 
							Controller.RightControllerTransform().position,
							rMidPoint.transform.position, 
							rLineRendererTarget.transform.position,
							lineSteps);
						break;
					case false:
						rightLr.SetPosition(0, Controller.RightControllerTransform().position);
						rightLr.SetPosition(1, rLineRendererTarget.transform.position);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			if (rightFocusObject == null || LeftManipulationStay || RightManipulationStay)
			{
				rightLr.enabled = false;
			}
		}
		private void DrawDebugLines()
		{
			if (Controller.debugActive == false) return;
			foreach (var g in objectsInGaze)
			{
				Debug.DrawLine(transform.position, g.transform.position, Color.cyan);
			}
			foreach (var g in objectsInManualLeft)
			{
				Debug.DrawLine(Controller.LeftControllerTransform().transform.position, g.transform.position, Color.blue);
			}
			foreach (var g in objectsInManualRight)
			{
				Debug.DrawLine(Controller.RightControllerTransform().transform.position, g.transform.position, Color.blue);
			}
			if (leftFocusObject != null)
			{
				Debug.DrawLine(lMidPoint.transform.position, lLineRendererTarget.transform.position, Color.red);
				Debug.DrawLine(Controller.LeftControllerTransform().position, lMidPoint.transform.position, Color.red);
				Debug.DrawLine(Controller.LeftControllerTransform().transform.position, leftFocusObject.transform.position, Color.green);
			}
			if (rightFocusObject != null)
			{
				Debug.DrawLine(rMidPoint.transform.position, rLineRendererTarget.transform.position, Color.red);
				Debug.DrawLine(Controller.RightControllerTransform().position, rMidPoint.transform.position, Color.red);
				Debug.DrawLine(Controller.RightControllerTransform().transform.position, rightFocusObject.transform.position, Color.green);
			}
		}
		#endregion
	}
}
