using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    [RequireComponent(typeof(ObjectSelection))]
    [RequireComponent(typeof(Manipulation))]
    public class Rotation : MonoBehaviour
    {/*
        private ObjectSelection c;
        private Manipulation m;
        private LineRenderer lr;
        private SelectableObject selectableObject;
        [SerializeField] private GameObject rot;
        [HideInInspector] public Transform target;
        [TabGroup("Rotation Behaviours")] [SerializeField] [Range(0, 10)] private float magnification = 2f;
        [TabGroup("Rotation Behaviours")] [Space(3)] public bool enableRotationSnapping;
        [TabGroup("Rotation Behaviours")] [ShowIf("EnableRotationSnapping")][Indent][Range(1, 90)] public int rotationSnapping = 1;
        private Quaternion previousRot;                  
        private Quaternion applyRot;                     
        [HideInInspector] public bool rotating;
 
        private void Start()
        {
            c = GetComponent<ObjectSelection>();
            m = GetComponent<Manipulation>();
            target = m.tSr.transform;
            rot = new GameObject("Rotation/Reference");
            lr = gameObject.AddComponent<LineRenderer>();
            Setup.LineRender(lr, c.Controller.lineRenderMat, .001f, false);
        }

        private void Update()
        {
            if (!c.LStay && !c.RStay)
            {
                RotationEnd();
            }
            else
            {
                if (c.Controller.RightGrab() && c.Controller.LeftGrab())
                {
                    RotationStart();
                    RotationStay();
                }
                else
                {
                    RotationEnd();
                }
            }
        }

        private void RotationStart()
        {
            if (c.grabObject == null || rot == null) return;
            if (rotating) return;
            rotating = true;        
            previousRot = rot.transform.rotation;
            lr.enabled = true;
            selectableObject = c.grabObject.GetComponent<SelectableObject>();
        }

        private void RotationStay()
        {
            DrawLineRenderer(lr, c.Controller.LeftControllerTransform(), c.Controller.RightControllerTransform());
            CalculateRotation();

            var rotation = rot.transform.rotation;
            var deltaRotation = previousRot * Quaternion.Inverse(rotation);
            var t = Quaternion.Inverse(Quaternion.LerpUnclamped(Quaternion.identity, deltaRotation, magnification)) * this.target.rotation;
        
            previousRot = rotation;
            switch (selectableObject.rotationLock)
            {
                case SelectableObject.RotationLock.FreeRotation:
                    if ((int) t.eulerAngles.y % rotationSnapping == 0)
                    {
                        target.rotation = Quaternion.Lerp(target.rotation, new Quaternion(t.x, t.y, t.z, t.w), .5f);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        
            previousRot = rot.transform.rotation;
        }

        private void RotationEnd()
        {
            lr.enabled = false;
            rotating = false;
        }

        private void CalculateRotation()
        {
            rot.gameObject.transform.position = new Vector3(
                (c.Controller.LeftControllerTransform().position.x +
                 c.Controller.RightControllerTransform().position.x) / 2,
                (c.Controller.LeftControllerTransform().position.y +
                 c.Controller.RightControllerTransform().position.y) / 2,
                (c.Controller.LeftControllerTransform().position.z +
                 c.Controller.RightControllerTransform().position.z) / 2);
            rot.transform.LookAt(c.Controller.RightControllerTransform());
        }

        private void DrawLineRenderer(LineRenderer l, Transform a, Transform b)
        {/*
            switch (c.controllerEnum)
            {     
                case ObjectSelection.ControllerEnum.Left:
                    c.rMidPoint.transform.localPosition = new Vector3(0, 0,
                        Set.Midpoint(c.Controller.LeftControllerTransform(), m.tS.transform));
                    BezierCurve.BezierLineRenderer(l,
                        c.Controller.RightControllerTransform().position,
                        c.rMidPoint.transform.position,
                        c.grabObject.transform.position,
                            c.quality);
                    break;
                case ObjectSelection.ControllerEnum.Right:
                    c.rMidPoint.transform.localPosition = new Vector3(0, 0,
                        Set.Midpoint(c.Controller.LeftControllerTransform(), m.tS.transform));
                    BezierCurve.BezierLineRenderer(l,
                        c.Controller.RightControllerTransform().position,
                        c.rMidPoint.transform.position,
                        c.grabObject.transform.position,
                        c.quality);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        */}
    }


/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ROTATIONTEST : MonoBehaviour
{
    public Transform A;
    public Transform B;

    public float scaler;
    
    private Quaternion _aPreviousRotation;
    private Quaternion _applyRot;
    private float _scalar;
    
    private void Start()
    {
        _aPreviousRotation = A.transform.rotation;
    }
    private void Update()
    {
        Quaternion deltaRotation = _aPreviousRotation * Quaternion.Inverse(A.transform.rotation);
        B.transform.rotation = Quaternion.Inverse(Quaternion.LerpUnclamped(Quaternion.identity, deltaRotation, scaler)) * B.transform.rotation;
        _aPreviousRotation = A.transform.rotation;
    }
}

 */