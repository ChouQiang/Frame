using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class UIEventController : MonoBehaviour
{
    Transform dragPanel;
    bool tapStart;
    Dictionary<int, FingerData> fingerPool = new Dictionary<int, FingerData>();
    public static UIEventController inst;
    private void Awake()
    {
        inst = this;
        dragPanel = transform.Find("dragPanel");
        EventListener.AddDrag(dragPanel, onDrag);
        EventListener.AddDown(dragPanel, onDown);
        EventListener.AddUp(dragPanel, onUp);
        EventListener.AddPinch(dragPanel, onPinch);

    }
    // Use this for initialization
    void Start()
    {

    }

    public int checkFingerDown()
    {
        int count = 0;
        if(fingerPool.Count <= 0)
        {
            return 0;
        }

        foreach(FingerData obj in fingerPool.Values)
        {
            if(obj.flag)
            {
                count++;
            }
        }
        return count;
    }

    public void onDrag(PointerEventData data)
    {
        if(!checkCanFinger())
        {
            return;
        }

        FingerData obj = getSameFinger(data);
        if(obj == null)
        {
            return;
        }

        tapStart = false;
        Vector3 prePosition = obj.prePosition;
        obj.prePosition = data.position;
        if(Input.touchCount > 2)
        {
            return;
        }

        if(checkFingerDown() != 1)
        {
            return;
        }

        if(data.delta.sqrMagnitude == 0)
        {
            return;
        }

        Vector3 nowPos = data.position;
        Ray ray = Camera.main.ScreenPointToRay(nowPos);
        RaycastHit hit;
        bool flag = Physics.Raycast(ray, out hit, Config.maxNum, 1 << LayerMask.NameToLayer("CameraDragLayer"));
        Vector3 pos2;
        if (flag)
        {
            pos2 = hit.point;
        }
        else
        {
            return;
        }

        Vector3 pos1;
        ray = Camera.main.ScreenPointToRay(prePosition);
        flag = Physics.Raycast(ray, out hit, Config.maxNum, 1 << LayerMask.NameToLayer("CameraDragLayer"));
        if (flag)
        {
            pos1 = hit.point;
        }
        else
        {
            return;
        }
        Vector3 pos3 = pos2 - pos1;
        Vector2 delta = new Vector2(pos3.x, pos3.z);
        CameraController.inst.dragMove(delta, 0.1f);
    }

    public void onDown(PointerEventData data)
    {
        if(!checkCanFinger())
        {
            return;
        }

        FingerData obj = new FingerData();
        obj.prePosition = data.position;
        obj.flag = true;
        obj.id = data.pointerId;
        fingerPool[data.pointerId] = obj;
        if(Util.checkIsOverUI(dragPanel.gameObject, obj.id))
        {
            obj.flag = false;
            return;
        }

        if(Input.touchCount >= 2)
        {
            return;
        }

        tapStart = true;
    }

    public void onUp(PointerEventData data)
    {
        if(!checkCanFinger())
        {
            return;
        }

        if(tapStart)
        {
            tapStart = false;
            onTap(data);
        }
        fingerPool.Remove(data.pointerId);
    }

    public void onTap(PointerEventData data)
    {

    }



    public void onPinch(PointerEventData data, float delta)
    {
        tapStart = false;
        CameraController.inst.pinchMove(delta * 3);
        FingerData obj = getSameFinger(data);
        if(obj == null)
        {
            return;
        }

        obj.prePosition = data.position;
    }

    FingerData getSameFinger(PointerEventData obj)
    {
        foreach(int id in fingerPool.Keys)
        {
            if(id == obj.pointerId)
            {
                return fingerPool[id];
            }
        }

        return null;
    }

    bool checkCanFinger()
    {
        return true;
    }

    private void OnDestroy()
    {
        EventListener.RemoveDrag(dragPanel.gameObject);
        EventListener.RemoveDown(dragPanel.gameObject);
        EventListener.RemoveUp(dragPanel.gameObject);
        EventListener.RemovePinch(dragPanel.gameObject);

        if(inst == this)
        {
            inst = null;
        }
    }
}

public class FingerData
{
    public bool flag;
    public BaseEventData data;
    public Vector3 prePosition;
    public int id;
}
