using Sirenix.OdinInspector;
using UnityEngine;
using Valve.VR;

namespace VR_Prototyping.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        private enum SDK
        {
            SteamVR,
            VRTK
        }
        
        [SerializeField] public bool debugActive;
        
        [TabGroup("Transforms")] [SerializeField] [Required] private Transform l;
        [TabGroup("Transforms")] [SerializeField] [Required] private Transform r;
        [TabGroup("Transforms")] [SerializeField] [Required] private Transform h;


        [TabGroup("Button Events")] [SerializeField] private SDK VR_SDK;
        [TabGroup("Button Events")] public SteamVR_Action_Boolean grabGrip;
        [TabGroup("Button Events")] public SteamVR_Action_Boolean triggerGrip;
        [TabGroup("Button Events")] public SteamVR_Action_Boolean joystickPress;
        [TabGroup("Button Events")] public SteamVR_Action_Vector2 joystickDirection;
        [TabGroup("Button Events")] public SteamVR_Action_Vibration haptic;
       
        [TabGroup("Aesthetics")][ SerializeField] [Required] public Material lineRenderMat;
        
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
        
        public bool LeftJoystickPress()
        {
            return joystickPress.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        public bool RightJoystickPress()
        {
            return joystickPress.GetState(SteamVR_Input_Sources.RightHand);
        }

        public Vector2 LeftJoystick()
        {
            return joystickDirection.GetAxis(SteamVR_Input_Sources.LeftHand);
        }

        public Vector2 RightJoystick()
        {
            return joystickDirection.GetAxis(SteamVR_Input_Sources.RightHand);
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

