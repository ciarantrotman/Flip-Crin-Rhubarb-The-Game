using UnityEngine;

namespace FlipCrinRob.Scripts
{
    public static class Setup
    {
        public static void LineRender(LineRenderer lr, Material m, float w, bool e)
        {
            lr.material = m;
            lr.startWidth = w;
            lr.endWidth = w;
            lr.useWorldSpace = true;
            lr.enabled = e;
        }
    }
}
