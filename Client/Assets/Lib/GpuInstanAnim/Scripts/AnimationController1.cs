using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController1 : MonoBehaviour {
    public BaseAnimation animation;
    // Use this for initialization
    public bool isBlend = false;
	void Start () {
        InvokeRepeating("changeAnimation", 0.1f, 3f);
       
	}

    void changeAnimation()
    {
        float r = Random.Range(0f, 1f);
        if (r > 0.5)
        {
            animation.changeAnimation("stand", 0f, isBlend);
        }
        else
        {
            animation.changeAnimation("walk", 0f, isBlend);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
