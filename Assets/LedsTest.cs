using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class LedsTest : MonoBehaviour
{
    public static LedsTest Instance { get; private set; }

    // pixel-related consts 
    public const int NumStrands = 8;
    public const int NumPixelsPerStrand = 480;

    // message-related consts
    private const int HeaderLength = 4;
    private const int NumBytesPerPixel = 3;
    private int PixelDataLength = NumStrands * NumPixelsPerStrand;
    private const int PixelDataInBytesPerMessage = NumStrands * NumPixelsPerStrand * NumBytesPerPixel;

    private int[] StrandMapping = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47 };

    public float CurrentColorLerp = 1.0f;
    public Color[] PixelColors;


    // networking related 
    public bool NetworkEnable = true;
    private int Port1 = 7895;
    public string HostIP1 = "10.0.0.106";
    private Thread UDPThreadOutput;
    private bool HandleUDPRequired = false;
    private float ThreadRuntime = 0.0f;
    private UdpClient MainUDPClient;
    private IPEndPoint TargetToSendTo1;
    private bool WritingMessage1 = false;
    public float NetworkThreadFPS { get; private set; }
    private long FrameCount = 0;
    private float NetworkThreadMaxFPS = 50.0f;
    private int MessageByteLength = 0;
    public int MessageCounter = 0;
    private byte[] Message1Bytes;


    void Start()
    {
        PixelColors = new Color[PixelDataLength];

        if (NetworkEnable)
        {
            // set-up LAN data 
            MainUDPClient = new UdpClient(Port1); // `new UdpClient()` to auto-pick port
            TargetToSendTo1 = new IPEndPoint(IPAddress.Parse(HostIP1), Port1);


            // set-up LAN thread 
            HandleUDPRequired = true;
            UDPThreadOutput = new Thread(new ThreadStart(HandleUDPOutput));
            UDPThreadOutput.IsBackground = true;
            UDPThreadOutput.Start();
        }

        MessageByteLength = 4 + PixelDataInBytesPerMessage;
        Message1Bytes = new byte[MessageByteLength];
    }


    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("multiple instances of LEDTreeManager - this instance will be destroyed");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        HandleUDPRequired = false;

        if (UDPThreadOutput != null)
            UDPThreadOutput.Abort();

        if (MainUDPClient != null)
            MainUDPClient.Close();
    }


    void CalcThreadFPS()
    {
        DateTime timePrev = DateTime.Now;

        ThreadRuntime += (float)(DateTime.Now - timePrev).TotalSeconds;
        float secondsElapsed = (float)(DateTime.Now - timePrev).TotalSeconds;
        float initialNetworkThreadFPS = 1.0f / (float)secondsElapsed;
        float targetSecondsSpent = (1.0f / NetworkThreadMaxFPS);
        if (initialNetworkThreadFPS > NetworkThreadMaxFPS)
        {
            float timeToSleep = targetSecondsSpent - secondsElapsed;
            //yield return new WaitForSeconds(timeToSleep);
            Thread.Sleep(Mathf.RoundToInt(1000.0f * timeToSleep));
        }
        else if (initialNetworkThreadFPS < 5.0f)
            Thread.Sleep(100);

        secondsElapsed = (float)(DateTime.Now - timePrev).TotalSeconds;
        float actualNetworkThreadFPS = 1.0f / (float)secondsElapsed;
        NetworkThreadFPS = actualNetworkThreadFPS;
        timePrev = DateTime.Now;
        FrameCount++;
    }

    void OnDisable()
    {
        if (UDPThreadOutput != null)
            UDPThreadOutput.Abort();

        if (MainUDPClient != null)
            MainUDPClient.Close();
    }


    void HandleUDPOutput()
    {
        ThreadRuntime = 0.0f;

        while (HandleUDPRequired)
        {
            if (MainUDPClient != null && Message1Bytes != null)
            {
                try
                {
                    if (!WritingMessage1)
                    {
                        MainUDPClient.Send(Message1Bytes, Message1Bytes.Length, TargetToSendTo1);
                        CalcThreadFPS();
                        MessageCounter++;
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError("Could not send UDP message: " + err.Message);
                }
            }
            else
            {
                Debug.LogError("Skipped UDP messages!");
                //    MainUDPClient = new UdpClient(Port1); // `new UdpClient()` to auto-pick port
            }
        }
    }


    public void OnApplicationQuit()
    {
        HandleUDPRequired = false;

        if (UDPThreadOutput != null)
            UDPThreadOutput.Abort();

        if (MainUDPClient != null)
            MainUDPClient.Close();
    }


    void UpdatePixels()
    {
        for (int currentRenderedStrand = 0; currentRenderedStrand < NumStrands; currentRenderedStrand++)
        {
            for (int currentRenderedPixelInStrand = 0; currentRenderedPixelInStrand < NumPixelsPerStrand; currentRenderedPixelInStrand++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("UpdatePumpPixelsLoop");

                int PixelIndex = currentRenderedStrand * NumPixelsPerStrand + currentRenderedPixelInStrand;
                Color pixelColor = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f);
                PixelColors[PixelIndex] = Color.Lerp(PixelColors[PixelIndex], pixelColor, CurrentColorLerp);

                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
    }

    void UpdateNetworkMessagesWithPixelColors()
    {
        for (int i = 0; i < StrandMapping.Length; i++)
        {
            if (i < NumStrands)
            {
                // set message strand based on surface mapping and surface-strand-offset 
                int messageStrand = StrandMapping[i];

                for (int pixelInStrand = 0; pixelInStrand < NumPixelsPerStrand; pixelInStrand++)
                {
                    int currentRenderedPixelInMessage = messageStrand * NumPixelsPerStrand + pixelInStrand;

                    Message1Bytes[HeaderLength + currentRenderedPixelInMessage * 3 + 0] = (byte)(1.0 * 254.0f);
                    Message1Bytes[HeaderLength + currentRenderedPixelInMessage * 3 + 1] = (byte)(0.0 * 254.0f);
                    Message1Bytes[HeaderLength + currentRenderedPixelInMessage * 3 + 2] = (byte)(0.0 * 254.0f);
                    /*
                    Message1Bytes[HeaderLength + currentRenderedPixelInMessage * 3 + 0] = (byte)(PixelColors[pixelInStrand].r * 254.0f);
                    Message1Bytes[HeaderLength + currentRenderedPixelInMessage * 3 + 1] = (byte)(PixelColors[pixelInStrand].g * 254.0f);
                    Message1Bytes[HeaderLength + currentRenderedPixelInMessage * 3 + 2] = (byte)(PixelColors[pixelInStrand].b * 254.0f);
                    */
                }
            }
        }
    }

    void UpdateMessagesHeaders()
    {
        // channel: 0 is for broadcasting the same message to all strands 
        Message1Bytes[0] = 0;

        // command: 0 is for Set Pixel Colors
        Message1Bytes[1] = 0;

        // data_length 
        ushort number = Convert.ToUInt16(PixelDataInBytesPerMessage);
        byte upper = (byte)(number >> 8);
        byte lower = (byte)(number & 0xff);
        Message1Bytes[2] = upper;
        Message1Bytes[3] = lower;
    }


    void Update()
    {
        UpdatePixels();

        WritingMessage1 = true;

        if (NetworkEnable)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UpdateNetworkMessagesWithPixelColors");
            UpdateNetworkMessagesWithPixelColors();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("UpdateMessagesHeaders");
            UpdateMessagesHeaders();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        WritingMessage1 = false;
    }
}