using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class ControllerTransforms : MonoBehaviour
{
    [SerializeField] private Transform _l;
    [SerializeField] private Transform _r;
    [SerializeField] private bool _debugActive;

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
}
