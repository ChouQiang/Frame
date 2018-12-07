using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ProjManager : MonoBehaviour
{
    public Transform dragPlane;
    bool NeedCameraControl = true;
    public Vector3 cameraLookPos;
    public Vector3 cameraRotation;
    public float DistanceOfCameraToSea = 30;
    public float CameraLeftLimitDis = 30;
    public float CameraRightLimitDis = 30;
    public float CameraForwardLimitDis = 30;
    public float CameraBackLimitDis = 30;
    public float CameraFarLimitDis = 5;
    public float CameraNearLimitDis = 5;

    public static ProjManager inst;
    private void Awake()
    {
        inst = this;
        GameObject obj = GameObject.Find("UICanvas");
        if(obj == null)
        {
            NeedCameraControl = false;
        }
        else
        {
            if(GameObject.Find("EventSystem") == null)
            {
                Debug.LogWarning("UGUI EventSystem can not be found in this scene. CameraController need UGUI EventSystem.");
            }
        }
        
    }



    void checkAddCameraControl()
    {
        Camera.main.gameObject.AddComponent<CameraController>();
    }

    // Use this for initialization
    void Start()
    {
        if (NeedCameraControl)
        {
            checkAddCameraControl();
        }
        Util.initSystem();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        if(inst == this)
        {
            inst = null;
        }
    }
}
