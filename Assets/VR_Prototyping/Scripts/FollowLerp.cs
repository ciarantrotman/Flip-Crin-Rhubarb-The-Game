using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
	public class FollowLerp : MonoBehaviour
	{
		[TabGroup("Follow Settings")][SerializeField]private Transform lerpTarget;
		[TabGroup("Follow Settings")][SerializeField][Range(0f,1f)]private float lerpSpeed;
		[Space(5)]
		[TabGroup("Lock Settings")][SerializeField] private bool lockXPosition;
		[TabGroup("Lock Settings")][SerializeField] private bool lockYPosition;
		[TabGroup("Lock Settings")][SerializeField] private bool lockZPosition;
		[Space(10)]
		[TabGroup("Lock Settings")][SerializeField] private bool lockXRotation;
		[TabGroup("Lock Settings")][SerializeField] private bool lockYRotation;
		[TabGroup("Lock Settings")][SerializeField] private bool lockZRotation;
		[TabGroup("Lock Settings")][SerializeField] private bool lockWRotation;
	
		private Vector3 defaultPosition;
		private Quaternion defaultRotation;

		private void Awake()
		{
			var transformTemp = transform;
			defaultPosition = transformTemp.localPosition;
			defaultRotation = transformTemp.rotation;
		}
	
		private void Update () 
		{
			Transform thisTransform;
			var thatTransform = lerpTarget.transform;
			var position = thatTransform.position;
			(thisTransform = transform).position = Vector3.Lerp(transform.position, new Vector3(
				lockXPosition ? defaultPosition.x : position.x,
				lockYPosition ? defaultPosition.y : position.y,
				lockZPosition ? defaultPosition.z : position.z), lerpSpeed);
			var transform3 = lerpTarget.transform;
			var rotation = transform3.rotation;
			transform.rotation = Quaternion.Lerp(thisTransform.rotation, new Quaternion(
				lockXRotation ? defaultRotation.x : rotation.x,
				lockYRotation ? defaultRotation.y : rotation.y,
				lockZRotation ? defaultRotation.z : rotation.z,
				lockWRotation ? defaultRotation.w : rotation.w), lerpSpeed);
		}
	}
}
