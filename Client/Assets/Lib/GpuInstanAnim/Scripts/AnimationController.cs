using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {
    public BaseAnimation animation;
	// Use this for initialization
	void Start () {
        float r = Random.Range(0f, 1f);
        if(r > 0.5)
        {
            animation.changeAnimation("stand", Random.Range(0f, 1f));
        }
        else
        {
            animation.changeAnimation("walk", Random.Range(0f, 1f));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
