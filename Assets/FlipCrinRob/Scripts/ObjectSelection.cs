using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(ControllerTransforms))]
	public class ObjectSelection : MonoBehaviour
	{
		#region 01 Inspector and Variables
		public ControllerTransforms Controller { get; private set; }
		public bool RStay { get; set; }
		public bool LStay { get; set; }
		private enum SelectionType
		{
			Fusion,
			Fuzzy,
			RayCast
		}
		private static bool TypeCheck(SelectionType type)
		{
			return type == SelectionType.Fusion;
		}
		private GameObject lTarget;
		private GameObject rTarget;
		private bool lSelectPrevious;
		private bool rSelectPrevious;
		private bool lGrabPrevious;
		private bool rGrabPrevious;
		private GameObject lDefault;
		private GameObject rDefault;
		private SelectableObject pLSelectableObject;
		private SelectableObject pRSelectableObject;
		
		[HideInInspector] public GameObject grabObject;
		[HideInInspector] public GameObject lMidPoint;
		[HideInInspector] public GameObject rMidPoint;
		[HideInInspector] public GameObject lFocusObject;
		[HideInInspector] public GameObject rFocusObject;
		[HideInInspector] public SelectableObject rSelectableObject;
		[HideInInspector] public SelectableObject lSelectableObject;
		[HideInInspector] public LineRenderer lLr;
		[HideInInspector] public LineRenderer rLr;
		[HideInInspector] public bool disableSelection;
		
		[ValidateInput("TypeCheck", "Recommended Selection Type is Fusion", InfoMessageType.Warning)]
		[BoxGroup("Selection Settings")] [SerializeField] private SelectionType selectionType;
		[BoxGroup("Selection Settings")] [HideIf("selectionType", SelectionType.RayCast)] [Indent] [Range(0f, 180f)] public float gaze = 60f;
		[BoxGroup("Selection Settings")] [HideIf("selectionType", SelectionType.RayCast)] [Indent] [Range(0f, 180f)] public float manual = 25f;
		[BoxGroup("Selection Settings")] [Space(10)]public bool setSelectionRange;		
		[BoxGroup("Selection Settings")] [ShowIf("setSelectionRange")] [Indent] [Range(0f, 250f)] public float selectionRange = 25f;		
		[BoxGroup("Selection Settings")] public bool disableLeftHand;
		[BoxGroup("Selection Settings")] public bool disableRightHand;
		
		[TabGroup("Object Lists")] public List<GameObject> globalList;
		[TabGroup("Object Lists")] public List<GameObject> gazeList;
		[TabGroup("Object Lists")] public List<GameObject> rHandList;
		[TabGroup("Object Lists")] public List<GameObject> lHandList;

		[TabGroup("Aesthetics")] [Range(3f, 30f)] public int lineRenderQuality = 15;
		[TabGroup("Aesthetics")] [Range(0f, 2.5f)] public float offset = 1f;
		
		#endregion
		private void Start ()
		{
			Controller = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			
			lLr = Controller.LeftControllerTransform().gameObject.AddComponent<LineRenderer>();
			rLr = Controller.RightControllerTransform().gameObject.AddComponent<LineRenderer>();
			
			Setup.LineRender(lLr, Controller.lineRenderMat, .005f, true);
			Setup.LineRender(rLr, Controller.lineRenderMat, .005f, true);
		}
		private void SetupGameObjects()
		{
			lMidPoint = new GameObject("MidPoint/Left");
			rMidPoint = new GameObject("MidPoint/Right");
			lTarget = new GameObject("TargetLineRender/Left");
			rTarget = new GameObject("Target/LineRender/Right");
			lDefault = new GameObject("Target/LineRender/Left/Default");
			rDefault = new GameObject("Target/LineRender/Right/Default");
						
			Setup.LineRenderObjects(lDefault.transform, Controller.LeftControllerTransform(), offset);
			Setup.LineRenderObjects(rDefault.transform, Controller.RightControllerTransform(), offset);
			
			Setup.LineRenderObjects(lMidPoint.transform, Controller.LeftControllerTransform(), 0f);
			Setup.LineRenderObjects(rMidPoint.transform, Controller.RightControllerTransform(), 0f);
		}
		
		private void Update () 
		{		
			SortLists();

			switch (selectionType)
			{
				case SelectionType.Fuzzy:
					lFocusObject = ObjectMethods.FuzzyFindFocusObject(lHandList, lFocusObject, lTarget, lDefault, Controller.LeftGrab());
					rFocusObject = ObjectMethods.FuzzyFindFocusObject(rHandList, rFocusObject, rTarget, rDefault, Controller.RightGrab());
					break;
				case SelectionType.RayCast:
					lFocusObject = ObjectMethods.RayCastFindFocusObject(lHandList, lFocusObject, lTarget, lDefault, Controller.LeftControllerTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.LeftGrab());
					rFocusObject = ObjectMethods.RayCastFindFocusObject(rHandList, rFocusObject, rTarget, rDefault, Controller.RightControllerTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.RightGrab());
					break;
				case SelectionType.Fusion:
					lFocusObject = ObjectMethods.FusionFindFocusObject(lHandList, lFocusObject, lTarget, lDefault, Controller.LeftControllerTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.LeftGrab());
					rFocusObject = ObjectMethods.FusionFindFocusObject(rHandList, rFocusObject, rTarget, rDefault, Controller.RightControllerTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.RightGrab());
					break;
				default:
					lFocusObject = null;
					rFocusObject = null;
					break;
			}
			
			lSelectableObject = ObjectMethods.FindSelectableObject(lFocusObject, lSelectableObject, Controller.LeftGrab());
			rSelectableObject = ObjectMethods.FindSelectableObject(rFocusObject, rSelectableObject, Controller.RightGrab());
			
			ObjectMethods.DrawLineRenderer(lLr, lFocusObject, lMidPoint, Controller.LeftControllerTransform(), lTarget, lineRenderQuality, Controller.LeftGrab());
			ObjectMethods.DrawLineRenderer(rLr, rFocusObject, rMidPoint, Controller.RightControllerTransform() ,rTarget, lineRenderQuality, Controller.RightGrab());
		}
		private void FixedUpdate()
		{
			ObjectMethods.Manipulation(lFocusObject, lSelectableObject, Controller.LeftGrab(), lGrabPrevious, Controller.LeftControllerTransform(), lMidPoint.transform, lTarget.transform);
			ObjectMethods.Manipulation(rFocusObject, rSelectableObject, Controller.RightGrab(), rGrabPrevious, Controller.RightControllerTransform(), rMidPoint.transform, rTarget.transform);
			
			lGrabPrevious = Controller.LeftGrab();
			rGrabPrevious = Controller.RightGrab();
		}

		private void LateUpdate()
		{
			ObjectMethods.Selection(lFocusObject, lSelectableObject, Controller.LeftSelect(), lSelectPrevious);
			ObjectMethods.Selection(rFocusObject, rSelectableObject, Controller.RightSelect(), rSelectPrevious);
			
			ObjectMethods.Hover(lSelectableObject, pLSelectableObject);
			ObjectMethods.Hover(rSelectableObject, pRSelectableObject);
		
			lSelectPrevious = Controller.LeftSelect();
			rSelectPrevious = Controller.RightSelect();

			pLSelectableObject = lSelectableObject;
			pRSelectableObject = rSelectableObject;
		}

		private void SortLists()
		{
			lHandList.Sort(SortBy.FocusObjectL);
			rHandList.Sort(SortBy.FocusObjectR);
		}
	}
}
