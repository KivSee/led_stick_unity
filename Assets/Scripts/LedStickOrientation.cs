using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LedStickOrientation : MonoBehaviour
{

    public Color32[] m_colors;
    private HSVColor[] m_additiveBuffer;
    public LedStickAnimation[] m_animations;

    ParticleSystem m_System;
    ParticleSystem.Particle[] m_Particles;
    public float lampSize = 0.05f;

    void Start()
    {
        InitializeIfNeeded();
        m_animations = GetComponents<LedStickAnimation>();
    }

    void FixedUpdate()
    {
        /*LedStickAnimation anToUse = null;
        foreach (LedStickAnimation an in m_animations) {
            if(an.enabled)
            {
                anToUse = an;
                break;
            }
        }*/

        CalculateAdditiveBuffer();

        for (int i = 0; i < m_Particles.Length; i++)
        {
            HSVColor currColor = m_additiveBuffer[i];
            m_colors[i] = Color.HSVToRGB(currColor.Hue % 1.0f, currColor.Sat, currColor.Val);
            m_Particles[i].startColor = m_colors[i];
            m_Particles[i].startSize = lampSize;
        }

        // Apply the particle changes to the particle system
        m_System.SetParticles(m_Particles, m_Particles.Length);
    }

    // sets the additive buffer, mixing all the colors from different animations together
    void CalculateAdditiveBuffer()
    {

        // first clean the buffer
        foreach(HSVColor c in m_additiveBuffer)
        {
            c.Clear();
        }

        // collect the relevant animations
        List<LedStickAnimation> relevantAnimations = new List<LedStickAnimation>();
        foreach (LedStickAnimation an in m_animations)
        {
            if(an.enabled)
            {
                relevantAnimations.Add(an);
            }
        }

        // create array only once to hold the relevant colors in each index
        HSVColor[] currColors = new HSVColor[relevantAnimations.Count];

        // copy the transperancy values of each animation, to use for blend
        float[] transparency = new float[relevantAnimations.Count];
        for(int i=0; i<relevantAnimations.Count; i++)
        {
            transparency[i] = relevantAnimations[i].transparency;
        }

        // create the array and blend the colors
        for (int i = 0; i < m_Particles.Length; i++)
        {
            // combine the colors
            for (int anI = 0; anI < relevantAnimations.Count; anI++)
            {
                currColors[anI] = relevantAnimations[anI].colorsArr[i];
            }
            m_additiveBuffer[i].SetFromBlendColorsArray(currColors, transparency);
        }

    }

    void InitializeIfNeeded()
    {
        if (m_System == null)
        {
            m_System = GetComponent<ParticleSystem>();
        }

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
        {
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
            for (int i = 0; i < m_Particles.Length / 2; i++)
            {
                m_Particles[i].position = new Vector3(-0.02f, 0.15f + i * (1.0f / 60.0f), 0.0f);

                m_Particles[m_Particles.Length - 1 - i].position = new Vector3(+0.02f, 0.15f + i * (1.0f / 60.0f), 0.0f);
            }
            m_System.SetParticles(m_Particles, m_Particles.Length);

            m_colors = new Color32[m_Particles.Length];
            m_additiveBuffer = new HSVColor[m_Particles.Length];
            for(int i=0; i< m_additiveBuffer.Length; i++)
            {
                m_additiveBuffer[i] = new HSVColor();
            }
        }
    }
}
