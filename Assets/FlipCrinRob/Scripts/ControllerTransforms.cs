using Sirenix.OdinInspector;
using UnityEngine;
using Valve.VR;

namespace FlipCrinRob.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        [SerializeField] public bool debugActive;
        
        [TabGroup("Transforms")][SerializeField] private Transform l;
        [TabGroup("Transforms")][SerializeField] private Transform r;
        [TabGroup("Transforms")][SerializeField] private Transform h;
        
        [TabGroup("SteamVR")]public SteamVR_Action_Boolean grabGrip;
        [TabGroup("SteamVR")]public SteamVR_Action_Boolean triggerGrip;
        [TabGroup("SteamVR")]public SteamVR_Action_Vibration haptic;
       
        [TabGroup("Aesthetics")][SerializeField] public Material lineRenderMat;
        
        public Transform LeftControllerTransform()
        {
            return l;
        }
    
        public Transform RightControllerTransform()
        {
            return r;
        }

        public Transform CameraTransform()
        {
            return h;
        }

        public Vector3 CameraPosition()
        {
            return h.position;
        }

        public bool LeftGrab()
        {
            return grabGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }
    
        public bool RightGrab()
        {
            return grabGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public bool LeftSelect()
        {
            return triggerGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        public bool RightSelect()
        {
            return triggerGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public Vector3 LeftForwardVector()
        {
            return l.transform.TransformVector(Vector3.forward);
        }
    
        public Vector3 RightForwardVector()
        {
            return r.transform.TransformVector(Vector3.forward);
        }

        public Vector3 CameraForwardVector()
        {
            return h.forward;
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

