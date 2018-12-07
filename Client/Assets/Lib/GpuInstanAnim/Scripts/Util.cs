using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class Util
{
    static PointerEventData pos;
    public static void initSystem()
    {
        pos = new PointerEventData(EventSystem.current);
    }
   
    public static bool checkIsOverUI(GameObject obj, int id)
    {
#if UNITY_EDITOR
        if (!Input.GetMouseButtonDown(0))
        {
            return false;
        }
        pos.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#else
        if(Input.touchCount <= 0)
        {
            return false;
        }
        pos.position = Input.GetTouch(id).position;
#endif
        List<RaycastResult> list = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pos, list);

        if (list.Count == 0)
        {
            return false;
        }

        if (list[0].gameObject != obj)
        {
            return true;
        }

        return false;
    }
}
