using UnityEngine;
using Valve.VR;

namespace FlipCrinRob.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        [SerializeField] private Transform l;
        [SerializeField] private Transform r;
        [SerializeField] public bool debugActive;
        [SerializeField] public Material lineRenderMat;
        public SteamVR_Action_Boolean grabGrip;
        public SteamVR_Action_Vibration haptic;
        
        public Transform LeftControllerTransform()
        {
            if (debugActive)
            {
                Debug.Log(l);
            }
            return l;
        }
    
        public Transform RightControllerTransform()
        {
            if (debugActive)
            {
                Debug.Log(r);
            }
            return r;
        }

        public bool LeftGrab()
        {
            if (debugActive)
            {
                Debug.Log("Left Grab: " + grabGrip.GetState(SteamVR_Input_Sources.LeftHand));
            }
            return grabGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }
    
        public bool RightGrab()
        {
            if (debugActive)
            {
                Debug.Log("Right Grab: " + grabGrip.GetState(SteamVR_Input_Sources.RightHand));
            }
            return grabGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public Vector3 LeftForwardVector()
        {
            return l.transform.TransformVector(Vector3.forward);
        }
    
        public Vector3 RightForwardVector()
        {
            return r.transform.TransformVector(Vector3.forward);
        }

        public SteamVR_Input_Sources LeftSource()
        {
            return SteamVR_Input_Sources.LeftHand;
        }
        
        public SteamVR_Input_Sources RightSource()
        {
            return SteamVR_Input_Sources.RightHand;
        }
    }
}

