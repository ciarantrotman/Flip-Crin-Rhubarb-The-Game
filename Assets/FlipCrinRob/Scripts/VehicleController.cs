using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms _controller;
        
        [SerializeField] private HandleController _lHandle;
        [SerializeField] private HandleController _cHandle;
        [SerializeField] private HandleController _rHandle;

        [SerializeField] private float _dual;
        [SerializeField] private float _mono;

        private void Start()
        {
            
        }

        private void Update()
        {
            _lHandle.ClipThreshold = _dual;
            _cHandle.ClipThreshold = _mono;
            _rHandle.ClipThreshold = _dual;
            
//            float Ll = _lHandle.HandleDistance(_controller.LeftControllerTransform());
//            float Lc = _cHandle.HandleDistance(_controller.LeftControllerTransform());
//            float Lr = _rHandle.HandleDistance(_controller.LeftControllerTransform());
//            float Rl = _lHandle.HandleDistance(_controller.RightControllerTransform());
//            float Rc = _cHandle.HandleDistance(_controller.RightControllerTransform());
//            float Rr = _rHandle.HandleDistance(_controller.RightControllerTransform());
//
//            Debug.Log(Ll + ", " + Lc + ", " + Lr + ", " + Rl + ", " + Rc + ", " + Rr);
        }
    }
}
