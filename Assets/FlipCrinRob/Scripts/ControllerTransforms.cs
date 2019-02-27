using UnityEngine;
using Valve.VR;

namespace FlipCrinRob.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        [SerializeField] private Transform _l;
        [SerializeField] private Transform _r;
        [SerializeField] private bool _debugActive;
    
        public SteamVR_Action_Boolean GrabGrip;

        public Transform LeftControllerTransform()
        {
            if (_debugActive)
            {
                Debug.Log(_l);
            }
            return _l;
        }
    
        public Transform RightControllerTransform()
        {
            if (_debugActive)
            {
                Debug.Log(_r);
            }
            return _r;
        }

        public bool LeftGrab()
        {
            if (_debugActive)
            {
                Debug.Log("Left Grab: " + GrabGrip.GetState(SteamVR_Input_Sources.LeftHand));
            }
            return GrabGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }
    
        public bool RightGrab()
        {
            if (_debugActive)
            {
                Debug.Log("Right Grab: " + GrabGrip.GetState(SteamVR_Input_Sources.RightHand));
            }
            return GrabGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public Vector3 LeftForwardvector()
        {
            return _l.transform.TransformVector(Vector3.forward);
        }
    
        public Vector3 RightForwardvector()
        {
            return _r.transform.TransformVector(Vector3.forward);
        }
    }
}

