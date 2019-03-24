using System.Collections.Generic;
using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class ObjectMethods
    {
        public static void Manipulation(Object focusObject, SelectableObject selectableObject, bool grip, bool pGrip, Transform con, Transform mid)
        {
            if (focusObject == null || selectableObject == null) return;
            if (grip && !pGrip)
            {
                selectableObject.GrabStart(con);
            }
            if (grip && pGrip)
            {
                selectableObject.GrabStay(con, mid);
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

        public static void Hover(Object focusObject, SelectableObject button, bool hover, bool pHover)
        {
            if (focusObject == null || button == null) return;
            if (hover && !pHover)
            {
                button.HoverStart();
            }
            if (hover && pHover)
            {
                button.HoverStay();
            }
            if (!hover && pHover)
            {
                button.HoverEnd();
            }
        }
        
        public static GameObject FindFocusObject(List<GameObject> objects, GameObject t, Transform d)
        {
            return objects.Count > 0 ? objects[0].gameObject : null;
        }

        public static SelectableObject FindSelectableObject(GameObject focusObject)
        {
            if (focusObject == null) return null;
            return focusObject.GetComponent<SelectableObject>() != null ? focusObject.GetComponent<SelectableObject>() : null;
        }
        
        public static void DrawLineRenderer(LineRenderer lr, GameObject focus, GameObject midpoint, GameObject inactive, Transform controller, GameObject target, int quality, bool disabled)
        {
            Set.LerpPosition(target.transform, focus != null ? focus.transform : inactive.transform, disabled ? .1f : .3f);
            midpoint.transform.localPosition = new Vector3(0, 0, Set.Midpoint(controller, target.transform));
            Set.LineRenderWidth(lr, .001f, focus != null ? .01f : .001f);
            
            BezierCurve.BezierLineRenderer(lr, 
                controller.position,
                midpoint.transform.position, 
                target.transform.position,
                quality);

            lr.enabled = !disabled;
        }
        
        public static void GrabLineRenderer(LineRenderer lr, Transform con, Transform mid, Transform target, int q)
        {
            mid.transform.localPosition = new Vector3(0, 0,Set.Midpoint(con, target));
            Set.LineRenderWidth(lr, .001f, .01f);
            BezierCurve.BezierLineRenderer(lr, 
                con.position,
                mid.position, 
                target.position,
                q);
        }
        
        public static void GrabStart(GameObject f, GameObject p, GameObject target, GameObject o, Transform con)
        {
            f.transform.LookAt(con);
            p.transform.position = con.position;
            p.transform.LookAt(target.transform);
            target.transform.SetParent(con);
            o.transform.position = con.position;
        }
    }
}
