using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRainbow : LedStickAnimation
{

    void Start()
    {

    }

    void FixedUpdate()
    {
        for (int i = 0; i < colorsArr.Length; i++)
        {
            colorsArr[i].Set((float)i / colorsArr.Length, 1.0f, 1.0f);
        }
    }
}
