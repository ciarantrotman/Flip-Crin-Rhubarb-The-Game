using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class Check
    {
        public static void Manipulation(Object focusObject, SelectableObject selectableObject, bool grip, bool pGrip, Transform con, Transform mid, Transform end)
        {
            if (focusObject == null || selectableObject == null) return;
            if (grip && !pGrip)
            {
                selectableObject.GrabStart(con);
            }
            if (grip && pGrip)
            {
                selectableObject.GrabStay(con, mid, end);
            }
            if (!grip && pGrip)
            {
                selectableObject.GrabEnd(con);
            }
        }
        
        public static void Selection(Object focusObject, SelectableObject button, bool select, bool pSelect)
        {
            if (focusObject == null || button == null) return;
            if (select && !pSelect)
            {
                button.SelectStart();
            }
            if (select && pSelect)
            {
                button.SelectStay();
            }
            if (!select && pSelect)
            {
                button.SelectEnd();
            }
        }

        public static void Hover(SelectableObject current, SelectableObject previous)
        {
            if (current != previous && current != null)
            {
                current.HoverStart();
            }
            if (current == previous && current != null)
            {
                current.HoverStay();
            }
            if (current != previous && previous != null)
            {
                previous.HoverEnd();
            }
        }

        public static void Locomotion(Locomotion t, bool current, bool previous, GameObject visual, LineRenderer lr)
        {
            if (t != null)
            if (current && !previous)
            {
                t.LocomotionStart(visual, lr);
            }
            if (current && previous)
            {
                t.LocomotionStay(visual);
            }
            if (!current && previous)
            {
                t.LocomotionEnd(visual, lr);
            }
        }
        
        public static GameObject RayCastFindFocusObject(List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, Transform controller, float distance, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            var position = controller.position;
            var forward = controller.forward;

            if (Physics.Raycast(position, forward, out var hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                Set.VectorLerpPosition(target.transform, hit.point, .15f);
                //Debug.DrawRay(position, forward * hit.distance, Color.green);
                return hit.transform.gameObject;
            }

            target.transform.SetParent(null);
            Set.TransformLerpPosition(target.transform, inactive.transform, .1f);
            //Debug.DrawRay(position, forward * distance, Color.red);
            return null;
        }
        
        public static GameObject FuzzyFindFocusObject(List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            Set.TransformLerpPosition(target.transform, objects.Count > 0 ? objects[0].gameObject.transform: inactive.transform, .1f);
            return objects.Count > 0 ? objects[0].gameObject : null;
        }
        
        public static GameObject FusionFindFocusObject(List<GameObject> objects, GameObject current, GameObject target,GameObject inactive, Transform controller, float distance, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            var position = controller.position;
            var forward = controller.forward;
            
            if (Physics.Raycast(position, forward, out var hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                Set.VectorLerpPosition(target.transform, hit.point, .25f);
                //Debug.DrawRay(position, forward * hit.distance, Color.green);
                return hit.transform.gameObject;
            }

            if (objects.Count > 0)
            {
                target.transform.SetParent(objects[0].gameObject.transform);
                Set.TransformLerpPosition(target.transform, objects[0].gameObject.transform, .25f);
                //Debug.DrawRay(position, forward * distance, Color.red);
                return objects[0].gameObject;
            }
            
            target.transform.SetParent(null);
            Set.TransformLerpPosition(target.transform, inactive.transform, .2f);
            //Debug.DrawRay(position, forward * distance, Color.red);
            return null;
        }

        public static SelectableObject FindSelectableObject(GameObject focusObject, SelectableObject current, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            if (focusObject == null) return null;
            return focusObject.GetComponent<SelectableObject>() != null ? focusObject.GetComponent<SelectableObject>() : null;
        }
        
        public static void DrawLineRenderer(LineRenderer lr, GameObject focus, GameObject midpoint, Transform controller, GameObject target, int quality, bool grab)
        {
            midpoint.transform.localPosition = new Vector3(0, 0, Set.Midpoint(controller, target.transform));
            Set.LineRenderWidth(lr, .001f, focus != null ? .01f : 0f);
            
            BezierCurve.BezierLineRenderer(lr, 
                controller.position,
                midpoint.transform.position, 
                target.transform.position,
                quality);
        }
        
        public static void GrabStart(GameObject f, GameObject p, GameObject target, GameObject o, Transform con)
        {
            f.transform.LookAt(con);
            p.transform.position = con.position;
            p.transform.LookAt(target.transform);
            target.transform.SetParent(con);
            o.transform.position = con.position;
        }
        
        public static void FocusObjectFollow(Transform focus, Transform con, Transform tar, Transform tarS, Transform objO, Transform conO, Transform objP, bool d)
        {
            if (focus.transform.gameObject == null || d) return;
			
            Set.Transforms(tar, focus);
            Set.Transforms(tarS, focus);
            Set.Transforms(objO, focus);
            Set.Position(conO, con);
            Set.Position(objP, con);
        }
    }
}
