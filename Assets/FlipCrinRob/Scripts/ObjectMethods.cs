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
        
        public static void Selection(Object focusObject, SelectableObject button, bool c, bool p)
        {
            if (focusObject == null || button == null) return;
            if (c != p && c)
            {
                button.OnSelect();
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
        
        public static void DrawLineRenderer(LineRenderer lr, GameObject f, GameObject m, Transform c, GameObject t, int q, bool d)
        {
            Set.LerpPosition(t.transform, f != null ? f.transform : c, .25f);
			
            if (f != null && !d)
            {
                lr.enabled = true;
                m.transform.localPosition = new Vector3(0, 0, Set.Midpoint(c, t.transform));
                BezierCurve.BezierLineRenderer(lr, 
                    c.position,
                    m.transform.position, 
                    t.transform.position,
                    q);
            }
            else if (f == null || d)
            {
                lr.enabled = false;
            }
        }
        
        public static void GrabLineRenderer(LineRenderer lR, Transform con, Transform mid, Transform target, int q)
        {
            mid.transform.localPosition = new Vector3(0, 0,Set.Midpoint(con, target));
            BezierCurve.BezierLineRenderer(lR, 
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
