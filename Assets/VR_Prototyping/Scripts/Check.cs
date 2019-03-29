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

        public static void JoystickTracking(List<Vector2> list, Vector2 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        
        public static void PositionTracking(List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
            Debug.DrawLine(list[0], list[list.Count - 1], Color.red);
        }
        
        private static void CullList(IList list, float sensitivity)
        {
            if (list.Count > sensitivity)
            {
                list.RemoveAt(0);
            }
        }
        
        public static void Locomotion(Locomotion t, bool current, bool previous, GameObject visual, LineRenderer lr)
        {
            if (t != null)
            if (current && !previous)
            {
                t.LocomotionStart(visual, lr);
            }
            if (!current && previous)
            {
                t.LocomotionEnd(visual, visual.transform.position, visual.transform.eulerAngles, lr);
            }
        }
        
        public static void GestureDetection(Locomotion l, Vector2 current, Vector2 previous, float rot, float speed, float triggerValue, float toleranceValue, GameObject visual, LineRenderer lr, bool currentTouch, bool previousTouch, bool disabled, bool locomotionActive)
        {
            if (disabled) return;
            
            var trigger = Mathf.Abs(current.x) > triggerValue || Mathf.Abs(current.y) > triggerValue;
            var tolerance = Mathf.Abs(previous.x) - 0 <= toleranceValue && Mathf.Abs(previous.y) - 0 <= toleranceValue;
            var triggerEnd = Mathf.Abs(current.x) - 0 <= toleranceValue && Mathf.Abs(current.y) - 0 <= toleranceValue;
            var toleranceEnd = Mathf.Abs(previous.x) > triggerValue || Mathf.Abs(previous.y) > triggerValue;

            var latch = trigger && toleranceEnd;
            
            if ((trigger && tolerance && !locomotionActive && !latch) || (currentTouch && !previousTouch && !locomotionActive))
            {
                if (current.x > triggerValue)
                {
                    Debug.Log(current.x + " RIGHT");
                    l.RotateUser(rot, speed);
                    
                }
                else if (current.x < -triggerValue)
                {
                    Debug.Log(current.x + " LEFT");
                    l.RotateUser(-rot, speed);
                }
                else if (current.y < -triggerValue)
                {
                    Debug.Log(current.x + " BACK");
                    l.RotateUser(180f, speed);
                }
            }
            else if ((currentTouch && previousTouch && !locomotionActive) || (trigger && toleranceEnd && !locomotionActive))
            {
                l.LocomotionStart(visual, lr);
            }
            else if ((triggerEnd && toleranceEnd && locomotionActive) || (!currentTouch && previousTouch && locomotionActive))
            {
                l.LocomotionEnd(visual, visual.transform.position, visual.transform.eulerAngles, lr);
            }
        }
        
        public static void Target(GameObject visual, GameObject parent, Transform normal, Vector2 pos, GameObject target)
        {
            visual.transform.LookAt(RotationTarget(pos, target));
            
            parent.transform.forward = normal.forward;
        }
        
        private static Transform RotationTarget(Vector2 pos, GameObject target)
        {
            target.transform.localPosition = Vector3.Lerp(target.transform.localPosition, new Vector3(pos.x, 0, pos.y), .1f);
            return target.transform;
        }
        
        public static float ControllerAngle(GameObject follow, GameObject proxy, GameObject normal, Transform controller, Transform head, bool debug)
        {
            Set.Position(proxy.transform, controller);
            Set.ForwardVector(proxy.transform, controller);
            Set.SplitRotation(proxy.transform, normal.transform, true);
            Set.SplitPosition(head, controller, follow.transform);
            follow.transform.LookAt(proxy.transform);

            if (!debug) return Vector3.Angle(normal.transform.forward, proxy.transform.forward);
            
            var normalForward = normal.transform.forward;
            var proxyForward = proxy.transform.forward;
            var position = proxy.transform.position;
            
            Debug.DrawLine(follow.transform.position, position, Color.red);
            Debug.DrawRay(normal.transform.position, normalForward, Color.blue);
            Debug.DrawRay(position, proxyForward, Color.blue);

            return Vector3.Angle(normalForward, proxyForward);
        }
        
        public static float CalculateDepth(float angle, float maxAngle, float minAngle, float max, float min, Transform proxy)
        {
            var a = angle;

            a = a > maxAngle ? maxAngle : a;
            a = a < minAngle ? minAngle : a;

            a = proxy.eulerAngles.x < 180 ? minAngle : a;
            
            var proportion = Mathf.InverseLerp(maxAngle, minAngle, a);
            return Mathf.Lerp(max, min, proportion);
        }
        
        public static void TargetLocation(GameObject target, GameObject hitPoint, Transform current)
        {
            var t = target.transform;
            var position = t.position;
            var up = t.up;
            hitPoint.transform.position = Vector3.Lerp(hitPoint.transform.position, Physics.Raycast(position, -up, out var hit) ? hit.point : current.position, .25f);
            hitPoint.transform.up = Physics.Raycast(position, -up, out var h) ? h.normal : current.transform.up;
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
                return hit.transform.gameObject;
            }

            target.transform.SetParent(null);
            Set.TransformLerpPosition(target.transform, inactive.transform, .1f);
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
                return hit.transform.gameObject;
            }

            if (objects.Count > 0)
            {
                target.transform.SetParent(objects[0].gameObject.transform);
                Set.TransformLerpPosition(target.transform, objects[0].gameObject.transform, .25f);
                return objects[0].gameObject;
            }
            
            target.transform.SetParent(null);
            Set.TransformLerpPosition(target.transform, inactive.transform, .2f);
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
