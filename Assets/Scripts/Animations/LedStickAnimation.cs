using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LedStickOrientation))]
public class LedStickAnimation : MonoBehaviour {

    public HSVColor[] colorsArr = new HSVColor[220];
    public float transparency = 1.0f;

    public LedStickAnimation()
    {
        for(int i=0; i<colorsArr.Length; i++)
        {
            colorsArr[i] = new HSVColor();
        }
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
