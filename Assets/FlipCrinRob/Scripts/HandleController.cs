using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace FlipCrinRob.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class HandleController : MonoBehaviour
    {
        [SerializeField] private ControllerTransforms _controller;
        [HideInInspector] public float ClipThreshold;
        private Renderer _r;

        private void Start()
        {
            _r = transform.GetComponent<Renderer>();
            _r.material.SetFloat("_ClipThreshold", ClipThreshold);
            Debug.Log(_r.material.shader);
            transform.localScale = new Vector3(ClipThreshold+ClipThreshold, ClipThreshold+ClipThreshold, ClipThreshold+ClipThreshold);
        }

        private void Update()
        {
            _r.material.SetVector("_LeftHand", _controller.LeftControllerTransform().position);
            _r.material.SetVector("_RightHand", _controller.RightControllerTransform().position);
        }

        public float HandleDistance(Transform x)
        {
            return Vector3.Distance(x.position, transform.position);
        }
    }
}

