using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public class RubberbandedObject : MonoBehaviour
    {
        [TabGroup("Script References")][SerializeField] private Transform target;
        [TabGroup("Rotation RubberBanding")][SerializeField] private bool rotationRubberBanding;
        [TabGroup("Rotation RubberBanding")][ShowIf("rotationRubberBanding")][SerializeField] private float angleThreshold;
        [TabGroup("Rotation RubberBanding")][ShowIf("rotationRubberBanding")][Space(10)][SerializeField][Range(0,1)] private float rotationLerpSpeed;
        [TabGroup("Position RubberBanding")][SerializeField] private bool positionRubberBanding;
        [TabGroup("Position RubberBanding")][ShowIf("positionRubberBanding")][Space(10)][SerializeField] private float distanceThreshold;
        [TabGroup("Position RubberBanding")][ShowIf("positionRubberBanding")][SerializeField][Range(0,1)] private float positionLerpSpeed;
   
        private float angle;
        private float distance;
        private bool rotTrigger;
        private bool posTrigger;
        private const float T = 3f;
    
        [SerializeField] private bool debug;
        private void Update()
        {
            DebugVisualisation();
        
            distance = Vector3.Distance(transform.position, target.position);
            angle = Vector3.Angle(transform.forward, target.forward);

            RotationRubberBanding();
            PositionRubberBanding();
        }

        private void RotationRubberBanding()
        {
            if (!rotationRubberBanding) return;
            if (angle >= angleThreshold && !rotTrigger)
            {
                rotTrigger = true;
            }
            if (rotTrigger && angle > T)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotationLerpSpeed);
            }
            else
            {
                rotTrigger = false;
            }
        }
    
        private void PositionRubberBanding()
        {
            if (!positionRubberBanding) return;
            if (distance >= distanceThreshold && !posTrigger)
            {
                posTrigger = true;
            }
            if (posTrigger && distance > T)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, positionLerpSpeed);
            }
            else
            {
                posTrigger = false;
            }
        }
    
        private void DebugVisualisation()
        {
            if (!debug) return;
            Debug.DrawRay(transform.position,transform.forward, Color.red);
            Debug.DrawRay(transform.position,transform.up, Color.red);
            Debug.DrawRay(transform.position,transform.right, Color.red);
            Debug.DrawRay(target.position,target.forward, Color.green);
            Debug.DrawRay(target.position,target.up, Color.green);
            Debug.DrawRay(target.position,target.right, Color.green);
            Debug.DrawLine(transform.position, target.position, Color.blue);
        }
    }
}

