using UnityEngine;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class HandleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms _controller;
        [HideInInspector] public float ClipThreshold;
        private Renderer _r;
        private enum Handle
        {
            Left,
            Right,
            Center
        }
        [SerializeField] private Handle _handle;

        private void Start()
        {
            Debug.Log(name + ": " + ClipThreshold);
            _r = transform.GetComponent<Renderer>();
            
            _r.material.SetFloat("_ClipThreshold", ClipThreshold);
            _r.material.SetFloat("_CutThreshold", ClipThreshold * .75f);
            transform.localScale = new Vector3(
                ClipThreshold + ClipThreshold, 
                ClipThreshold + ClipThreshold, 
                ClipThreshold + ClipThreshold);
        }

        private void Update()
        {
            _r.material.SetVector("_LeftHand", _controller.LeftControllerTransform().position);
            _r.material.SetVector("_RightHand", _controller.RightControllerTransform().position);

            switch (_handle)
            {
                case Handle.Left:
                    if (_distance(_controller.LeftControllerTransform()) <= ClipThreshold)
                    {
                        _r.material.SetInt("_Activated", _controller.LeftGrab() ? 0 : 1);
                    }
                    else
                    {
                        _r.material.SetInt("_Activated", 1);
                    }
                    break;
                case Handle.Right:
                    if (_distance(_controller.RightControllerTransform()) <= ClipThreshold)
                    {
                        _r.material.SetInt("_Activated", _controller.RightGrab() ? 0 : 1);
                    }
                    else
                    {
                        _r.material.SetInt("_Activated", 1);
                    }
                    break;
                case Handle.Center:
                    Debug.Log("Hold your horses captain");
                    break;
            }
        }
        
        private float _distance(Transform c)
        {
            return Vector3.Distance(transform.position, c.position);
        }
        
        public float HandleDistance(Transform x)
        {
            return Vector3.Distance(x.position, transform.position);
        }
    }
}

