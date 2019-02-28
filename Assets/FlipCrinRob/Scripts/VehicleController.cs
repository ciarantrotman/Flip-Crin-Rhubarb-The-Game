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

        private bool lActive;
        private bool cActive;
        private bool rActive;

        private const float R = 5f;

        private const float HoverHeight = .2f;
        private const float HoverForce = 50f;

        private void Start()
        {
            _lHandle.ClipThreshold = _dual;
            _cHandle.ClipThreshold = _mono;
            _rHandle.ClipThreshold = _dual;
        }

        private void Update()
        {
            lActive = _lHandle.Active;
            cActive = _cHandle.Active;
            rActive = _rHandle.Active;
        }

        private void FixedUpdate()
        {
            Hover();
            
            if (lActive && rActive)
            {
                DualMovement();
            }
            else if (cActive)
            {
                MonoMovement();
            }
        }

        private void Hover()
        {
            Ray ray = new Ray (transform.position, -transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, HoverHeight))
            {
                float proportionalHeight = (HoverHeight - hit.distance) / HoverHeight;
                Vector3 appliedHoverForce = Vector3.up * proportionalHeight * HoverForce;
                _rb.AddForce(appliedHoverForce, ForceMode.Acceleration);
            }
        }

        private void DualMovement()
        {
            _l.localRotation = _controller.LeftControllerTransform().localRotation;
            _r.localRotation = _controller.RightControllerTransform().localRotation;
            var averageRotation = _v.localRotation;
            
            averageRotation = Quaternion.Lerp(averageRotation, 
                Quaternion.Lerp(
                    _controller.LeftControllerTransform().localRotation, 
                    _controller.RightControllerTransform().localRotation, 
                    .5f), 
                .1f);
            
            _v.localRotation = averageRotation;

            _rb.AddForce(_controller.LeftForwardvector() * _lHandle.M);
            _rb.AddForce(_controller.RightForwardvector() * _rHandle.M);               
            _rb.AddTorque(transform.up * averageRotation.y * R);
                
            _speedText.SetText((averageRotation.y * R).ToString());
        }

        private void MonoMovement()
        {
            _rb.AddForce(transform.TransformVector(transform.forward) * _cHandle.M);
        }
    }
}
