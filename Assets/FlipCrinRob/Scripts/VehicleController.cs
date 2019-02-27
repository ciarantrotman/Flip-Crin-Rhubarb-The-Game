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

        private bool _lActive;
        private bool _cActive;
        private bool _rActive;
        
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

            if (_lActive && _rActive)
            {
                _rb.AddForce(new Vector3(0, 1, 30));
            }
        }
    }
}
