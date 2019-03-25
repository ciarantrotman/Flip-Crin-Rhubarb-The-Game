using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class SortBy
    {
        public static int FocusObjectL(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<SelectableObject>().AngleL.CompareTo(obj2.GetComponent<SelectableObject>().AngleL);
        }
        public static int FocusObjectR(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<SelectableObject>().AngleR.CompareTo(obj2.GetComponent<SelectableObject>().AngleR);
        }
    }
}
