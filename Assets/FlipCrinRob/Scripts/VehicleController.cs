using TMPro;
using UnityEngine;
using Valve.VR;

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

        [SerializeField] private Rigidbody _rb;

        [SerializeField] private TextMeshPro _speedText;

        [SerializeField] private Transform _l;
        [SerializeField] private Transform _r;
        [SerializeField] private Transform _v;

        private bool _lActive;
        private bool _cActive;
        private bool _rActive;

        private const float M = 10f;
        private const float R = 5f;
        
        private void Start()
        {
            _lHandle.ClipThreshold = _dual;
            _cHandle.ClipThreshold = _mono;
            _rHandle.ClipThreshold = _dual;
        }

        private void Update()
        {
            _lActive = _lHandle.Active;
            _cActive = _cHandle.Active;
            _rActive = _rHandle.Active;
        }

        private void FixedUpdate()
        {
            if (_lActive && _rActive)
            {
                DualMovement();
            }
            else if (_cActive)
            {
                MonoMovement();
            }
        }

        private void DualMovement()
        {
            _l.localRotation = _controller.LeftControllerTransform().localRotation;
            _r.localRotation = _controller.RightControllerTransform().localRotation;
            _v.localRotation = Quaternion.Lerp(_v.localRotation, 
                Quaternion.Lerp(
                    _controller.LeftControllerTransform().localRotation, 
                    _controller.RightControllerTransform().localRotation, 
                    .5f), 
                .1f);
                
            _rb.AddForce(_controller.LeftForwardvector() * M);
            _rb.AddForce(_controller.RightForwardvector() * M);               
            _rb.AddTorque(transform.up * _v.localRotation.y * R);
                
            _speedText.SetText((_v.localRotation.y * R).ToString()); //_controller.LeftForwardvector().ToString());
        }

        private void MonoMovement()
        {
            _rb.AddForce(transform.TransformVector(transform.forward) * M);
        }
    }
}
