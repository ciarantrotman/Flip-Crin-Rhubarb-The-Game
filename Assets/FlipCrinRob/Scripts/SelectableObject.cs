using System;
using System.Collections;
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
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;
		
		[TabGroup("Script Setup")] [SerializeField] private GameObject player;
		[TabGroup("Script Setup")] [SerializeField] private bool button;
		[TabGroup("Script Setup")] public bool toolTip;
		[ShowIf("toolTip")][TabGroup("Script Setup")][Indent] public string toolTipText;
		[ShowIf("button")][TabGroup("Script Setup")][SerializeField] private bool menu;
		[ShowIf("button")][ShowIf("menu")][Indent][TabGroup("Script Setup")] public GameObject menuItems;
		[HideIf("button")][Header("Define Object Behaviour")]
		[HideIf("button")][TabGroup("Object Behaviour")][SerializeField] public float moveSpeed = 1f;
		[HideIf("button")][TabGroup("Object Behaviour")][SerializeField] public bool throwable;
		public enum AxisLock
		{
			MovementDisabled,
			FreeMovement
		}
		[HideIf("button")][TabGroup("Manipulation Settings")] [Space(3)] [SerializeField] public bool directGrab = true;
		[HideIf("button")][TabGroup("Manipulation Settings")] [ShowIf("directGrab")] [SerializeField] [Indent] [Range(.1f, 5f)] private float directGrabDistance = .15f;
		[HideIf("button")][TabGroup("Manipulation Settings")] public bool freeManipulationEnabled = true;
		public AxisLock positionLock;
		public enum RotationLock
		{
			FreeRotation
		}
		[HideIf("button")][TabGroup("Rotation Settings")] [SerializeField] public bool freeRotationEnabled;
		[HideIf("button")][TabGroup("Rotation Settings")] [HideIf("freeRotationEnabled")] [Indent] [SerializeField] public RotationLock rotationLock;
		[ShowIf("button")] [TabGroup("Button Settings")] public TextMeshPro buttonText;
		[ShowIf("button")] [TabGroup("Button Settings")] public Renderer buttonBack;
		[Space(10)][ShowIf("button")][TabGroup("Button Settings")] [SerializeField] private UnityEvent onSelect;
		[ShowIf("button")][TabGroup("Button Settings")] [SerializeField] private UnityEvent _onHover;
		[ShowIf("button")][TabGroup("Button Settings")] [SerializeField] private UnityEvent _onHoverEnd;
		private RotationLock rotLock;
		private GameObject _instantiatedAxes;
		private IndirectObjectSelection c;
		private FreeManipulation freeManipulation;
		//private FreeRotation _freeRotation;
		private float gazeAngle;
		[HideInInspector] public float manualRightAngle;
		[HideInInspector] public float manualLeftAngle;
		[HideInInspector] public bool beingGrabbed;
		private float manualFrustumRef;
		private Rigidbody rb;
		private Vector3[] positions = {new Vector3(0,0,0), new Vector3(0,0,0)};	// used for throwing calculations
		private bool _throw;
		private bool _gravity;
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
			RemoveFromList();
		}
		private void InitialiseSelectableObject()
		{
			CheckPlayer();
			AssignComponents();
			SetupRigidBody();
			SetupManipulation();
			AddToList();
		}
		private void CheckPlayer()
		{
			if (player != null && player.GetComponent<IndirectObjectSelection>() != null &&
			    player.GetComponent<FreeManipulation>() != null) return;
			Debug.Log("Make sure the right scripts are attached to the VR Player");
			GetComponent<SelectableObject>().enabled = false;
			Destroy(this);
		}
		private void AssignComponents()
		{
			c = player.GetComponent<IndirectObjectSelection>();
			freeManipulation = player.GetComponent<FreeManipulation>();
			//_freeRotation = _player.GetComponent<FreeRotation>();
		}
		private void SetupRigidBody()
		{
			rb = GetComponent<Rigidbody>();
			rb.freezeRotation = true;
			rb.useGravity = button ? false : _gravity;
		}
		private void SetupManipulation()
		{
			if (freeManipulationEnabled)
			{
				positionLock = AxisLock.FreeMovement;
			}
			if (freeRotationEnabled)
			{
				rotationLock = RotationLock.FreeRotation;
			}
		}
		private void AddToList()
		{
			c = player.GetComponent<IndirectObjectSelection>();
			if (!c.allSelectableObjects.Contains(gameObject))
			{
				c.allSelectableObjects.Add(gameObject);
			}
		}
		private void RemoveFromList()
		{
			if (c.allSelectableObjects.Contains(gameObject))
			{
				c.allSelectableObjects.Remove(gameObject);
			}
			if (c.objectsInManualLeft.Contains(gameObject))
			{
				c.objectsInManualLeft.Remove(gameObject);
			}
			if (c.objectsInManualRight.Contains(gameObject))
			{
				c.objectsInManualRight.Remove(gameObject);
			}
		}
		private void Update()
		{
			SelectionRange();
			GetAngles();
			CheckDirectGrab();
			CheckIfInGaze();
			CheckIfInLeftManual();
			CheckIfInRightManual();
			OnHoverEnd();
		}
		private void FixedUpdate()
		{
			ThrowObject();
		}
		private void GetAngles()
		{
			var position = transform.position;
			gazeAngle = Vector3.Angle(position - c.Controller.CameraPosition(), c.Controller.CameraForwardVector());
			manualLeftAngle = Vector3.Angle(position - c.Controller.LeftControllerTransform().position, c.Controller.LeftForwardVector());
			manualRightAngle = Vector3.Angle(position - c.Controller.RightControllerTransform().position, c.Controller.RightForwardVector());
		}
		public void OnHover()
		{
			_onHover.Invoke();
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.2f, 1.2f,1.2f), .5f);
		}
		private void OnHoverEnd()
		{
			if(c.rightFocusObject == gameObject || c.leftFocusObject == gameObject) return;
			_onHoverEnd.Invoke();
			c.leftHandText.renderer.enabled = false;
			c.rightHandText.renderer.enabled = false;
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1f, 1f,1f), .5f);
		}
		public void OnSelect()
		{
			onSelect.Invoke();
		}
		private void SelectionRange()
		{
			if (!c.setSelectionRange) return;
		
			if (Vector3.Distance(transform.position, player.transform.position) >= c.selectionRange)
			{
				if (c.allSelectableObjects.Contains(gameObject))
				{
					c.allSelectableObjects.Remove(gameObject);
				}
			}
			else if (Vector3.Distance(transform.position, player.transform.position) < c.selectionRange)
			{
				if (!c.allSelectableObjects.Contains(gameObject))
				{
					c.allSelectableObjects.Add(gameObject);
				}
			}
		}
		private void CheckDirectGrab()
		{
			if (Vector3.Distance(transform.position, c.Controller.LeftControllerTransform().position) <= directGrabDistance)
			{
				c.activeObject = gameObject;
			}
		}
		private void CheckIfInGaze()
		{
			if (gazeAngle < c.gazeFrustumAngle/2 && c.objectsInGaze.Contains(gameObject) == false)
			{
				c.objectsInGaze.Add(gameObject);
			}
			else if (gazeAngle > c.gazeFrustumAngle/2)
			{
				c.objectsInGaze.Remove(gameObject);
			}
		}
		private void CheckIfInLeftManual()
		{
			if(freeManipulation.disableLeftHand && !button) return;
		
			if (c.objectsInGaze.Contains(gameObject) == false)
			{
				c.objectsInManualLeft.Remove(gameObject); //make it non selectable when you're not looking at it
				return;
			}
			if (manualLeftAngle < c.manualFrustumAngle/2 && c.objectsInManualLeft.Contains(gameObject) == false  && c.leftGrabObject != this)
			{
				c.objectsInManualLeft.Add(gameObject);
			}
			else if (manualLeftAngle > c.gazeFrustumAngle/2)
			{
				c.objectsInManualLeft.Remove(gameObject);
			}
		}
		private void CheckIfInRightManual()
		{
			if(freeManipulation.disableRightHand && !button) return;
			
			if (c.objectsInGaze.Contains(gameObject) == false)
			{
				c.objectsInManualRight.Remove(gameObject); //make it non selectable when you're not looking at it
				return;
			}
			if (manualRightAngle < c.manualFrustumAngle/2 && c.objectsInManualRight.Contains(gameObject) == false && c.rightGrabObject != this)
			{
				c.objectsInManualRight.Add(gameObject);
			}
			else if (manualRightAngle > c.gazeFrustumAngle/2)
			{
				c.objectsInManualRight.Remove(gameObject);
			}
		}
		public void IndirectManipulationStart()
		{
			if (c.LeftManipulationStay || c.RightManipulationStay) return;
			StopCoroutine(ThrowEnd());
			_throw = false;
		
			beingGrabbed = true;
		
			defaultPosition = transform.position;
			defaultLocalPosition = transform.localPosition;
			c.activeObject = gameObject;
		
			manualFrustumRef = c.manualFrustumAngle;
		
			c.manualFrustumAngle = 0f; 	// stop new objects from being selected when you're grabbing

			rb.mass = moveSpeed;
			rb.drag = moveSpeed * 5f;
			rb.angularDrag = moveSpeed * 5f;
			rb.velocity = new Vector3(0,0,0);
			rb.useGravity = false;

			switch (c.controllerEnum)
			{
				case IndirectObjectSelection.ControllerEnum.Left:
					c.leftHandText.renderer.enabled = true;
					c.LeftManipulationStay = true;
					break;
				case IndirectObjectSelection.ControllerEnum.Right:
					c.rightHandText.renderer.enabled = true;
					c.RightManipulationStay = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		
			if (positionLock == AxisLock.FreeMovement)
			{
				freeManipulation.OnStart();
			}
		}
		public void IndirectManipulationStay()
		{
			if (!beingGrabbed)
			{
				IndirectManipulationEnd();
				return;
			}
			if (directGrab)
			{
				switch (c.controllerEnum)
				{
					case IndirectObjectSelection.ControllerEnum.Left:
						if (Vector3.Distance(transform.position, c.Controller.LeftControllerTransform().position) <= directGrabDistance)
						{
							transform.position = Vector3.Lerp(transform.position, c.Controller.LeftControllerTransform().position, moveSpeed);
							transform.rotation = Quaternion.Lerp(transform.rotation, c.Controller.LeftControllerTransform().rotation, moveSpeed);
							return;
						}
						else
						{
							break;
						}
					case IndirectObjectSelection.ControllerEnum.Right:
						if (Vector3.Distance(transform.position, c.Controller.RightControllerTransform().position) <= directGrabDistance)
						{
							transform.position = Vector3.Lerp(transform.position, c.Controller.RightControllerTransform().position, moveSpeed);	
							transform.rotation = Quaternion.Lerp(transform.rotation, c.Controller.RightControllerTransform().rotation, moveSpeed);
							return;
						}
						else
						{
							break;
						}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			switch (positionLock)
			{
				case AxisLock.MovementDisabled:
					break;
				case AxisLock.FreeMovement:
					freeManipulation.OnStay();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

//			if (_freeRotation.Rotating)
//			{
//				transform.rotation = Quaternion.Lerp(transform.rotation, _freeRotation.Target.rotation, MoveSpeed);
//			}

			switch (c.controllerEnum)
			{
				case IndirectObjectSelection.ControllerEnum.Left:
					c.leftHandText.SetText(name + ": {0:1}", Vector3.Distance(transform.position, c.Controller.LeftControllerTransform().position));
					break;
				case IndirectObjectSelection.ControllerEnum.Right:
					c.rightHandText.SetText(name + ": {0:1}", Vector3.Distance(transform.position, c.Controller.RightControllerTransform().position));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			ThrowVelocity(transform);
		}
		public void IndirectManipulationEnd()
		{
			if(!throwable)
			{
				rb.velocity = new Vector3(0,0,0);	// stop the object from moving when you let go of it
			}
			else
			{
				_throw = true;
			}
			c.manualFrustumAngle = manualFrustumRef; 		// allow objects to be selected again
			c.objectsInGaze.Clear();
			c.LeftManipulationStay = false;
			c.RightManipulationStay = false;
			c.activeObject = null;
			c.leftHandText.renderer.enabled = false;
			c.rightHandText.renderer.enabled = false;
			rb.useGravity = _gravity;
			
			if (freeManipulationEnabled)
			{
				freeManipulation.OnEnd();
			}
		}
		private void ThrowObject()
		{
			if (!_throw) return;
			rb.AddForce(((positions[0] - positions[1]) / Time.deltaTime), ForceMode.Impulse);
			StartCoroutine(ThrowEnd());
		}
		private IEnumerator ThrowEnd()
		{
			if (rb.velocity.magnitude > 0)
			{
				var velocity = rb.velocity;
				velocity = new Vector3(velocity.x / 10, velocity.y / 10, velocity.z / 10);
				rb.velocity = velocity;
			}
			else
			{
				yield return null;
				_throw = false;
			}
		}
		private void ThrowVelocity(Transform controller)
		{
			positions[1] = positions[0];
			positions[0] = controller.position;
		}
	}
}
