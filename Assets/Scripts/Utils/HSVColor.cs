using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HSVColor {

    public float Hue;
    public float Sat;
    public float Val;

    public void Set(float Hue, float Sat, float Val)
    {
        this.Hue = Hue;
        this.Sat = Sat;
        this.Val = Val;
    }

    public void Clear()
    {
        this.Hue = 0.0f;
        this.Sat = 1.0f;
        this.Val = 0.0f;
    }

    public void CopyFromOther(HSVColor other)
    {
        this.Hue = other.Hue;
        this.Sat = other.Sat;
        this.Val = other.Val;
    }

    public void SetFromBlendColorsArray(HSVColor[] colors, float[] transparencies)
    {
        float transparencySum = 0.0f;
        foreach(float transparency in transparencies)
        {
            transparencySum += transparency;
        }

        // combine hues
        double x = 0.0;
        double y = 0.0;
        for(int i=0; i<colors.Length; i++)
        {
            float currTransperency = transparencies[i];
            double currHueRadians = colors[i].Hue * 2.0 * Math.PI;
            x += (Math.Cos(currHueRadians) * currTransperency);
            y += (Math.Sin(currHueRadians) * currTransperency);
        }
        this.Hue = (float)(Math.Atan2(y, x) / (2 * Math.PI));
        while(this.Hue < 0.0)
        {
            this.Hue += 1.0f;
        }

        // combine saturation
        this.Sat = 1.0f;

        // combine value
        this.Val = 1.0f;
    }

}
