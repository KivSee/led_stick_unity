using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TestParticalSystem : MonoBehaviour {

    public Color32[] m_colors;

    ParticleSystem m_System;
    ParticleSystem.Particle[] m_Particles;
    public float lampSize = 0.05f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        InitializeIfNeeded();

        float t = Time.fixedTime;
        for (int i = 0; i < m_Particles.Length; i++)
        {
            float hue = ( (i % 2 == 0) ? (t) : (t + 0.5f));
            Color32 currPixelColor = Color.HSVToRGB( hue % 1.0f, 1.0f, 1.0f);
            m_Particles[i].startColor = currPixelColor;
            m_Particles[i].startSize = lampSize;
            m_colors[i] = currPixelColor;
        }

        // Apply the particle changes to the particle system
        m_System.SetParticles(m_Particles, m_Particles.Length);
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
            for(int i=0; i<m_Particles.Length / 2; i++)
            {
                m_Particles[i].position = new Vector3(-0.02f, 0.15f + i * (1.0f / 60.0f), 0.0f);

                m_Particles[m_Particles.Length - 1 - i].position = new Vector3(+0.02f, 0.15f + i * (1.0f / 60.0f), 0.0f);
            }
            m_System.SetParticles(m_Particles, m_Particles.Length);

            m_colors = new Color32[m_Particles.Length];
        }
    }
}
