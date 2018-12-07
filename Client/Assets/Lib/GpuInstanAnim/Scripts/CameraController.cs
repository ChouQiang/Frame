using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController inst;

    CameraState state = CameraState.none;
    Vector3 dragOff = Vector3.zero;
    Transform plane;
    ProjManager manager;
    public Vector3 originLookPos;
    Vector3 nowOff;

    public float weight = 0.1f;
    float leftRange;
    float rightRange;
    float forwardRange;
    float backRange;
    float leftrightRange = 0;
    float forwardbackRange = 0;
    float farRange;
    float nearRange;
    float farnearRange = 0;
    float pinchOff = 0;
    float originSize;
    private void Awake()
    {
        inst = this;
        manager = GameObject.Find("Manager").GetComponent<ProjManager>();
        plane = manager.dragPlane;
        plane.gameObject.SetActive(true);
        initPos();
        initRange();
    }

    void initPos()
    {
        originLookPos = manager.cameraLookPos;
        transform.rotation = Quaternion.Euler(manager.cameraRotation);
        Vector3 off = -transform.forward * manager.DistanceOfCameraToSea;
        nowOff = off;
        transform.position = off + originLookPos;
    }

    private void OnDestroy()
    {
        if(inst == this)
        {
            inst = null;
        }
    }
    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        updateDragOff();
        updatePlane();
        updatePinchOff();
    }

    void updateDragOff()
    {
        if(dragOff.sqrMagnitude < 0.01)
        {
            return;
        }

        Vector3 oldPos = transform.position;
        Vector3 newPos = Vector3.Lerp(transform.position, transform.position + dragOff, weight);
        Vector3 subPos = newPos - oldPos;
        transform.position = oldPos + subPos;
        addLookPos(subPos);
        dragOff -= subPos;
    }

    void updatePlane()
    {
        plane.position = originLookPos;
    }

    void setLookPos(Vector3 pos)
    {
        dragOff = Vector3.zero;
        originLookPos.x = pos.x;
        originLookPos.z = pos.z;
        updatePlane();
    }

    void setState(CameraState state)
    {
        this.state = state;
    }

    void cancelState(Camera state)
    {
        this.state = CameraState.none;
    }

    void addLookPos(Vector3 off)
    {
        originLookPos += off;
    }

    void initRange()
    {
        farRange = manager.CameraFarLimitDis;
        nearRange = -manager.CameraNearLimitDis;
        leftRange = -manager.CameraLeftLimitDis;
        rightRange = manager.CameraRightLimitDis;
        forwardRange = -manager.CameraForwardLimitDis;
        backRange = manager.CameraBackLimitDis;
        originSize = Camera.main.orthographicSize;
    }

    public void dragMove(Vector2 delta, float speed)
    {
        if(!checkCanMove())
        {
            return;
        }

        _dragMove(delta, speed);
    }

    bool checkCanMove()
    {
        return state == CameraState.none;
    }

    void _dragMove(Vector2 delta, float speed)
    {
        Vector3 v1 = new Vector3(delta.x, 0, delta.y);
        float fMove = -Vector3.Dot(v1, Vector3.forward);
        float xMove = -Vector3.Dot(v1, Vector3.right);
        float padding = Config.CameraPadding;
        leftrightRange -= xMove;
        if(xMove > 0 && leftrightRange < leftRange - padding || xMove < 0 && leftrightRange > rightRange + padding)
        {
            leftrightRange += xMove;
            delta += new Vector2(Vector3.right.x, Vector3.right.z) * xMove;
        }
        forwardbackRange -= fMove;
        if(fMove > 0 && forwardbackRange < forwardRange - padding || fMove < 0 && forwardbackRange > backRange + padding)
        {
            forwardbackRange += fMove;
            delta += new Vector2(Vector3.forward.x, Vector3.forward.z) * fMove;
        }

        dragOff -= new Vector3(delta.x, 0, delta.y);
        weight = speed;
       
    }


    public void pinchMove(float x)
    {
        if(!checkCanMove())
        {
            return;
        }

        farnearRange += x;
        float padding = Config.CameraPadding;
        if(farnearRange < nearRange - padding)
        {
            farnearRange -= x;
            return;
        }

        if(farnearRange > farnearRange + padding)
        {
            farnearRange -= x;
            return;
        }

        pinchOff += x;
    }

    void updatePinchOff()
    {
        if(UIEventController.inst && UIEventController.inst.checkFingerDown() == 0)
        {
            float adjust = 0;
            if(farnearRange < nearRange)
            {
                adjust = farnearRange - nearRange;
                farnearRange = nearRange;
            }
            else if(farnearRange > farRange)
            {
                adjust = farnearRange - farRange;
                farnearRange = farRange;
            }

            pinchOff -= adjust;
        }

        if(Mathf.Abs(pinchOff) < 0.0001)
        {
            return;
        }


        Camera.main.transform.position -= pinchOff * weight * Camera.main.transform.forward;
       
        pinchOff = pinchOff - pinchOff * weight;
    }
}

public enum CameraState
{
    none = 0
}
