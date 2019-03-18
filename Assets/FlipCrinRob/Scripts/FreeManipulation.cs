using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(IndirectObjectSelection))]
	public class FreeManipulation : MonoBehaviour
	{
		#region Inspector and Variables
		private IndirectObjectSelection c;
		private GameObject controllerProxy;
		private GameObject objectProxy;	
		private GameObject controllerOriginal;
		private GameObject objectOriginal;
		private GameObject freeMovement;
		private GameObject player;
		private GameObject target;
		public GameObject targetScaled;
		private GameObject controllerFollow;
		private LineRenderer manipulationLineRenderer;
		private float initialDistance;
		private float m;
		private float z;
		
		[TabGroup("Indirect Manipulation")] [SerializeField] private bool objectMovesWithYouTeleport;
		[TabGroup("Indirect Manipulation")] [SerializeField] public bool disableRightHand;
		[TabGroup("Indirect Manipulation")] [SerializeField] public bool disableLeftHand;
		[TabGroup("Snap Settings")] [SerializeField] private bool distanceSnapping;
		[TabGroup("Snap Settings")] [ShowIf("distanceSnapping")] [Indent] [SerializeField] private float snapDistance = .15f;
		#endregion
		private void Start () 
		{
			c = GetComponent<IndirectObjectSelection>();
			player = c.gameObject;
			manipulationLineRenderer = controllerProxy.gameObject.AddComponent<LineRenderer>();
			Setup.LineRender(manipulationLineRenderer, c.Controller.lineRenderMat, .005f, false);
			SetupGameObjects();
			if (objectMovesWithYouTeleport)
			{
				freeMovement.transform.parent = player.transform;
			}	
		}

		private void SetupGameObjects()
		{
			controllerProxy = new GameObject("ControllerProxy");
			objectProxy = new GameObject("ObjectProxy");
			controllerOriginal = new GameObject("ControllerOriginal");
			objectOriginal = new GameObject("ObjectOriginal");
			freeMovement = new GameObject("FreeMovement");
			target = new GameObject("Target");
			targetScaled = new GameObject("TargetScaled");
			targetScaled.transform.SetParent(target.transform);
			controllerFollow = new GameObject("ControllerFollow");
		}
		private void Update()
		{	
			DrawDebugLines();
			FocusObjectFollow(c.leftFocusObject, c.LeftManipulationStay, c.RightManipulationStay, c.leftFocusObject.transform, c.Controller.LeftControllerTransform().position);
			FocusObjectFollow(c.rightFocusObject, c.LeftManipulationStay, c.RightManipulationStay, c.rightFocusObject.transform, c.Controller.RightControllerTransform().position);
			controllerFollow.transform.position = new Vector3(c.Controller.CameraPosition().x, ControllerFollowHeight(), c.Controller.CameraPosition().z);
		}
		public void OnStart()
		{
			switch (c.controllerEnum)
			{
				case IndirectObjectSelection.ControllerEnum.Left:
					controllerFollow.transform.LookAt(c.Controller.LeftControllerTransform());
					controllerProxy.transform.position = c.Controller.LeftControllerTransform().position;
					controllerProxy.transform.LookAt(target.transform);
					target.transform.SetParent(c.Controller.LeftControllerTransform());
					controllerOriginal.transform.position = c.Controller.LeftControllerTransform().position;
					break;
				case IndirectObjectSelection.ControllerEnum.Right:			
					controllerFollow.transform.LookAt(c.Controller.RightControllerTransform());
					controllerProxy.transform.position = c.Controller.RightControllerTransform().position;
					controllerProxy.transform.LookAt(target.transform);
					target.transform.SetParent(c.Controller.RightControllerTransform().transform);
					controllerOriginal.transform.position = c.Controller.RightControllerTransform().position;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var position = c.activeObject.transform.position;
			
			target.transform.position = position;
			target.transform.rotation = c.activeObject.transform.rotation;

			targetScaled.transform.position = target.transform.position;	
			targetScaled.transform.rotation = target.transform.rotation;
		
			objectProxy.transform.position = position;
			objectOriginal.transform.position = position;
		
			manipulationLineRenderer.enabled = true;
		}
		public void OnStay() 
		{		
			ControllerFollowing();
			DrawLineRender();
		
			m = (controllerProxy.transform.localPosition.z / controllerOriginal.transform.localPosition.z);			// the percentage change in the controller local.z

			if (m < 1f)
			{
				z = (objectOriginal.transform.localPosition.z * (m*m));														
			}
			else
			{
				z = (objectOriginal.transform.localPosition.z * (m*m*m));									
			}
		
			targetScaled.transform.localPosition = new Vector3(0, 0, z);									// scaling the local.z of the target based on the z depth change of the controller
		
			DistanceSnap();
		}
		public void OnEnd()
		{
			manipulationLineRenderer.enabled = false;
			target.transform.SetParent(null);
		}
		private void DistanceSnap()
		{
			switch (distanceSnapping) // decides the behaviour of the object snapping to its original distance
			{
				case true:
					c.activeObject.transform.position = Vector3.Lerp(
						c.activeObject.transform.position,
						Mathf.Abs(m - 1) > snapDistance ? targetScaled.transform.position : target.transform.position,
						.05f);
					break;
				case false:
					c.activeObject.transform.position = Vector3.Lerp(
						c.activeObject.transform.position,
						targetScaled.transform.position,
						.05f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		private float ControllerFollowHeight()
		{
			Vector3 ground = new Vector3(c.Controller.CameraPosition().x, 0f, c.Controller.CameraPosition().z);
			return Vector3.Distance(ground, c.Controller.CameraPosition()) * .6f;
		}

		private void FocusObjectFollow(Object s, bool l, bool r, Transform f, Vector3 p)
		{
			if (s == null || l || r) return;
			
			var pos = f.position;
			var rot = f.rotation;
				
			target.transform.position = pos;
			target.transform.rotation = rot;
				
			targetScaled.transform.position = target.transform.position;
			targetScaled.transform.rotation = target.transform.rotation;
				
			objectOriginal.transform.position = pos;
			objectOriginal.transform.rotation = rot;
				
			controllerOriginal.transform.position = p;
				
			objectProxy.transform.position = pos;
		}
		private void ControllerFollowing()
		{
			switch (c.controllerEnum)
			{
				case IndirectObjectSelection.ControllerEnum.Left:
					controllerFollow.transform.LookAt(c.Controller.LeftControllerTransform());
					controllerProxy.transform.position = c.Controller.LeftControllerTransform().position;
					controllerProxy.transform.LookAt(target.transform);
					break;
				case IndirectObjectSelection.ControllerEnum.Right:
					controllerProxy.transform.position = c.Controller.RightControllerTransform().position;
					controllerProxy.transform.LookAt(target.transform);
					controllerFollow.transform.LookAt(c.Controller.RightControllerTransform());
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private void DrawLineRender()
		{
			switch (c.controllerEnum)
			{
				case IndirectObjectSelection.ControllerEnum.Left:				
					c.lMidPoint.transform.localPosition = new Vector3(0, 0,
						MidpointDepth(c.Controller.LeftControllerTransform(), targetScaled.transform));
					BezierCurve.BezierLineRenderer(manipulationLineRenderer, 
						c.Controller.LeftControllerTransform().position,
						c.lMidPoint.transform.position, 
						c.activeObject.transform.position,
						c.lineSteps);
					break;
				case IndirectObjectSelection.ControllerEnum.Right:
					c.rMidPoint.transform.localPosition = new Vector3(0, 0, 
						MidpointDepth(c.Controller.RightControllerTransform(), targetScaled.transform));
					BezierCurve.BezierLineRenderer(manipulationLineRenderer, 
						c.Controller.RightControllerTransform().position,
						c.rMidPoint.transform.position,
						c.activeObject.transform.position,
						c.lineSteps);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		private static float MidpointDepth(Transform a, Transform b)
		{
			return Vector3.Distance(a.position, b.position) / 2;
		}
		private void DrawDebugLines()
		{
			if (c.Controller.debugActive == false) return;
			var position = objectProxy.transform.position;
			var position1 = objectOriginal.transform.position;
			var position2 = controllerOriginal.transform.position;
			var position3 = controllerProxy.transform.position;
			Debug.DrawLine(position3, position2, Color.red);
			Debug.DrawLine(position, position1, Color.red);
			Debug.DrawLine(position, position3, Color.blue);
			Debug.DrawLine(position1, position2, Color.blue);
			var position4 = targetScaled.transform.position;
			Debug.DrawLine(position, position4, Color.green);
			Debug.DrawLine(position3, position4, Color.green);
			Debug.DrawLine(target.transform.position, position4, Color.yellow);
		}
	}
}