using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
   public static class BezierCurve
   {
      public static void BezierLineRenderer(LineRenderer lr, Vector3 p0, Vector3 p1, Vector3 p2, int segments)
      {
         lr.positionCount = segments;
         lr.SetPosition(0, p0);
         lr.SetPosition(segments - 1, p2);
         
         for (var i = 1; i < segments; i++)
         {
            var point = GetPoint(p0, p1, p2, i / (float) segments);
            lr.SetPosition(i, point);
         }
      }
      private static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
      {
         return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
      }
   }
   /*
    *    public class BezierCurve : MonoBehaviour
   {
      private Vector3[] points =
      {
         new Vector3(0, 0, 0),
         new Vector3(0, 0, 0),
         new Vector3(0, 0, 0)
      };

      public void BezierLineRenderer(LineRenderer lr, Vector3 p0, Vector3 p1, Vector3 p2, int segments)
      {
         lr.positionCount = segments;
         lr.SetPosition(0, p0);
         lr.SetPosition(segments - 1, p2);
         points[0] = p0;
         points[1] = p1;
         points[2] = p2;
         
         for (int i = 1; i < segments; i++)
         {
            Vector3 point = GetPoint(i / (float) segments);
            lr.SetPosition(i, point);
         }
      }

      private Vector3 GetPoint(float t)
      {
         return (GetPoint(points[0], points[1], points[2], t));
      }

      private static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
      {
         return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
      }
   }
    */
}

