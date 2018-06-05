using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMapperLogic : MonoBehaviour {

    public LBClientSender controller1;
    public LedStickOrientation ledStick1;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        controller1.SetData(0, 0, ledStick1.m_colors);
	}
}
