using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class Setup
    {
        public static void LineRender(LineRenderer lr, Material m, float w, bool e)
        {
            lr.material = m;
            lr.startWidth = w;
            lr.endWidth = w;
            lr.numCapVertices = 10;
            lr.useWorldSpace = true;
            lr.enabled = e;
        }
        
        public static void LineRenderObjects(Transform m, Transform p, float offset)
        {
            m.position = p.position;
            m.parent = p;
            m.localRotation = new Quaternion(0,0,0,0);
            var position = m.position;
            m.localPosition = new Vector3(position.x, position.y, position.z + offset);
        }

        public static void SphereCollider(SphereCollider sc, bool trigger, float radius)
        {
            sc.isTrigger = trigger;
            sc.radius = radius;
        }
    }
}
