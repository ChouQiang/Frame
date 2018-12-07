using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
public class EventListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public Action<PointerEventData> onClick;
    public Action<PointerEventData> onDown;
    public Action<PointerEventData> onEnter;
    public Action<PointerEventData> onExit;
    public Action<PointerEventData> onUp;
    public Action<BaseEventData> onSelect;
    public Action<BaseEventData> onUpdateSelect;

    public Action<PointerEventData> onDragBegin;
    public Action<PointerEventData> onDrag;
    public Action<PointerEventData> onDragEnd;

    public Action<PointerEventData, float> onPinch;

    static public EventListener Get(GameObject go)
    {
        EventListener listener = go.GetComponent<EventListener>();
        if (listener == null) listener = go.AddComponent<EventListener>();
        return listener;
    }

    public static void AddClick(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onClick = func;
    }

    public static void RemoveClick(GameObject go)
    {
        EventListener listner = Get(go);
        listner.onClick = null;
    }

    public static void AddDown(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onDown = func;
    }

    public static void RemoveDown(GameObject go)
    {
        EventListener listner = Get(go);
        listner.onDown = null;
    }

    public static void AddUp(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onUp = func;
    }

    public static void RemoveUp(GameObject go)
    {
        EventListener listner = Get(go);
        listner.onUp = null;
    }

    public static void AddEnter(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onEnter = func;
    }

    public static void AddExit(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onExit = func;
    }

    public static void AddSelect(GameObject go, Action<BaseEventData> func)
    {
        EventListener listner = Get(go);
        listner.onSelect = func;
    }

    public static void AddUpdateSelect(GameObject go, Action<BaseEventData> func)
    {
        EventListener listner = Get(go);
        listner.onUpdateSelect = func;
    }

    public static void AddClick(Transform go, Action<PointerEventData> func)
    {
        AddClick(go.gameObject, func);
    }

    public static void AddDown(Transform go, Action<PointerEventData> func)
    {
        AddDown(go.gameObject, func);
    }

    public static void AddUp(Transform go, Action<PointerEventData> func)
    {
        AddUp(go.gameObject, func);
    }

    public static void AddEnter(Transform go, Action<PointerEventData> func)
    {
        AddEnter(go.gameObject, func);
    }

    public static void AddExit(Transform go, Action<PointerEventData> func)
    {
        AddExit(go.gameObject, func);
    }

    public static void AddSelect(Transform go, Action<BaseEventData> func)
    {
        AddSelect(go.gameObject, func);
    }

    public static void AddUpdateSelect(Transform go, Action<BaseEventData> func)
    {
        AddUpdateSelect(go.gameObject, func);
    }

    public static void AddDragBegin(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onDragBegin = func;
    }

    public static void AddDragBegin(Transform go, Action<PointerEventData> func)
    {
        AddDragBegin(go.gameObject, func);
    }

    public static void AddDrag(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onDrag = func;
    }

    public static void RemoveDrag(GameObject go)
    {
        EventListener listner = Get(go);
        listner.onDrag = null;
    }

    public static void AddDrag(Transform go, Action<PointerEventData> func)
    {
        AddDrag(go.gameObject, func);
    }

    public static void AddDragEnd(GameObject go, Action<PointerEventData> func)
    {
        EventListener listner = Get(go);
        listner.onDragEnd = func;
    }

    public static void AddDragEnd(Transform go, Action<PointerEventData> func)
    {
        AddDragEnd(go.gameObject, func);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
        {
            onClick(eventData);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        posDic[eventData.pointerId] = eventData.position;
        if (onDown != null)
        {
            onDown(eventData);
        }

        
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        posDic.Remove(eventData.pointerId);
        if (onUp != null) onUp(eventData);
    }
    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(eventData);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if(onDragBegin != null)
        {
            onDragBegin(eventData);
        }
    }




    public static void AddPinch(Transform go, Action<PointerEventData, float> func)
    {
        AddPinch(go.gameObject, func);
    }

    public static void AddPinch(GameObject go, Action<PointerEventData, float> func)
    {
        EventListener listener = Get(go);
        listener.onPinch = func;
    }

    public static void RemovePinch(GameObject go)
    {
        EventListener listener = Get(go);
        listener.onPinch = null;
    }

    Dictionary<int, Vector2> posDic = new Dictionary<int, Vector2>();
    public override void OnDrag(PointerEventData eventData)
    {
        if (Input.touchCount >= 2)
        {
            Vector2[] oldPosArr = new Vector2[2];
            int Count = 0;
            Vector2[] nowPosArr = new Vector2[2];
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 pos;
                bool flag = posDic.TryGetValue(touch.fingerId, out pos);
                if (flag)
                {
                    oldPosArr[Count] = pos;
                    nowPosArr[Count] = touch.position;
                    Count++;
                    if (Count == 2)
                    {
                        break;
                    }
                }
            }
            posDic[eventData.pointerId] = eventData.position;
            if (Count == 2)
            {
                float dis = zoomOff(oldPosArr[0], oldPosArr[1], nowPosArr[0], nowPosArr[1]);
                if (onPinch != null || Mathf.Abs(dis) > 0.1f)
                {
                    onPinch(eventData, -dis * 0.012f);
                    return;
                }
            }

        }
        if (onDrag != null)
        {
            onDrag(eventData);
        }

        
    }

    float zoomOff(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        float leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        float leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        return leng2 - leng1;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if(onDragEnd != null)
        {
            onDragEnd(eventData);
        }
    }

    public override void OnScroll(PointerEventData eventData)
    {
        if(onPinch != null)
        {
            onPinch(eventData, -eventData.scrollDelta.y * 0.2f);
        }
    }
}
