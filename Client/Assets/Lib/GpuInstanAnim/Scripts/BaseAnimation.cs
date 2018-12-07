using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BaseAnimation : MonoBehaviour
{
    class animationData
    {
        public float startNum;
        public float endNum;
        public bool isLoop;
    }
    public string animationFileName = "Warriorprefab";
    Renderer render;
    Dictionary<string, animationData> dic = new Dictionary<string, animationData>();
    private void Awake()
    {
        render = transform.GetComponent<Renderer>();
        frezzblock = new MaterialPropertyBlock();
        blendblock = new MaterialPropertyBlock();
        TextAsset text = Resources.Load("AnimationData/" + animationFileName + "AnimationData") as TextAsset;
        string[] arr1 = text.text.Split('\n');
        for(int i = 0; i < arr1.Length; i++)
        {
            string[] arr = arr1[i].Split('|');
            if(arr.Length < 4)
            {
                continue;
            }
            animationData data = new animationData();
            data.startNum = float.Parse(arr[1]);
            data.endNum = float.Parse(arr[2]);
            data.isLoop = bool.Parse(arr[3]);
            dic[arr[0]] = data;
        }
        
    }

    bool isLoop = false;
    float nowTime = 0;
    float maxTime = 0;
    bool isFrezz = false;
    public void Frezz()
    {
        isFrezz = true;
    }

    public void Resume()
    {
        isFrezz = false;
    }
    bool isBlend = false;
    public float blendSpeed = 0.8f;
    public void changeSpeed(float s)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        render.GetPropertyBlock(block);
        float start = block.GetFloat("_AnimStart");
        float end = block.GetFloat("_AnimEnd");
        float off = block.GetFloat("_AnimOff");
        float speed = block.GetFloat("_Speed");
        float _AnimLen = (end - start);
        float f = (off + Time.timeSinceLevelLoad * speed) / _AnimLen;
        float newf = (off + Time.timeSinceLevelLoad * s) / _AnimLen;
        float diff = newf - f;
        off = off + diff * _AnimLen;
        block.SetFloat("_AnimOff", off);
        block.SetFloat("_Speed", s);
    }

    public void changeAnimation(string animationName, float off = 0, bool needBlend = false)
    {
        animationData data = dic[animationName];
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        render.GetPropertyBlock(block);
        nowTime = -Time.timeSinceLevelLoad;
        float startNum = data.startNum;
        float endNum = data.endNum;
        maxTime = endNum - startNum;
        isLoop = data.isLoop;
        if(needBlend)
        {
            float oldAnimStart =  block.GetFloat("_AnimStart");
            float oldAnimEnd = block.GetFloat("_AnimEnd");
            float oldAnimOff = block.GetFloat("_AnimOff");
            block.SetFloat("_OldAnimStart", oldAnimStart);
            block.SetFloat("_OldAnimEnd", oldAnimEnd);
            block.SetFloat("_OldAnimOff", oldAnimOff);
            block.SetFloat("_Blend", 0f);
            isBlend = true;
        }
        block.SetFloat("_AnimStart", startNum);
        block.SetFloat("_AnimEnd", endNum);
        block.SetFloat("_AnimOff", nowTime + off);
        render.SetPropertyBlock(block);
    }
    MaterialPropertyBlock frezzblock;
    MaterialPropertyBlock blendblock;
    private void Update()
    {
        if(isFrezz)
        {
            render.GetPropertyBlock(frezzblock);
            float _AnimOff = frezzblock.GetFloat("_AnimOff");
            _AnimOff -= Time.deltaTime;
            frezzblock.SetFloat("_AnimOff", _AnimOff);
            render.SetPropertyBlock(frezzblock);
        }

        if(isBlend)
        {
            render.GetPropertyBlock(blendblock);
            float _Blend = blendblock.GetFloat("_Blend");
            _Blend += Time.deltaTime * blendSpeed;
            if(_Blend >= 1)
            {
                _Blend = 1;
                isBlend = false;
            }
            blendblock.SetFloat("_Blend", _Blend);
            render.SetPropertyBlock(blendblock);
        }
    }
    

}


