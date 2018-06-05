using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAlternate : LedStickAnimation
{

    public uint pixelsPerSegment = 2;
    public bool sidesCrossed = true;
    public float changeFhaseTimeSeconds = 1.0f;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        uint moduloValue = (pixelsPerSegment > 0) ? (2 * pixelsPerSegment) : 1;

        float t = Time.timeSinceLevelLoad;
        bool timeModuloValue = false;
        if (changeFhaseTimeSeconds != 0)
        {
            timeModuloValue = (t % (2.0f * changeFhaseTimeSeconds)) < changeFhaseTimeSeconds;
        }

        for (int side = 0; side < 2; side++)
        {
            bool sideModuloValue = sidesCrossed && (side == 1);

            int sideIndexMultiplier = (side == 0 ? 1 : -1);
            int sideIndexStart = (side == 0 ? 0 : (colorsArr.Length - 1));
            int pixelsPerSide = colorsArr.Length / 2;
            for (int i = 0; i < pixelsPerSide; i++)
            {
                bool indexModuloValue = (i % moduloValue < pixelsPerSegment);
                bool combinedModuloValue = timeModuloValue ^ indexModuloValue ^ sideModuloValue;

                float hue = (combinedModuloValue ? (0.0f) : (0.3f));
                int index = sideIndexStart + sideIndexMultiplier * i;
                colorsArr[index].Set(hue, 1.0f, 1.0f);
            }
        }
    }
}
