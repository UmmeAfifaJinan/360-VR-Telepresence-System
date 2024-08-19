using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TV_streaming : MonoBehaviour
{
    public RawImage display;
    public bool enableAudio = false;
    private string camName;
    private AudioSource audioSource;

    void Start()
    {
        ReadSteamingCamera();
        ProcessStreamingAudio(enableAudio);
        ProcessStreamingTexture();
    }

    private void ReadSteamingCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices; // Get devices list
        Debug.Log("Number of web cams connected: " + devices.Length);
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(i + " " + devices[i].name);
        }
        camName = devices[1].name;
        Debug.Log($"I am using the webcam named {camName}");
    }

    private void ProcessStreamingTexture()
    {
        WebCamTexture mycam = new WebCamTexture(camName);
        display.texture = mycam; 
        mycam.Play();
    }

    private void ProcessStreamingAudio(bool enableAudio)
    {
        audioSource = GetComponent<AudioSource>();
        string[] audioDevices = Microphone.devices;


        for (int i = 0; i < audioDevices.Length; i++)
        {
            Debug.Log(i + " " + audioDevices[i]);
        }

        if (enableAudio)
        {
            audioSource.clip = Microphone.Start(audioDevices[0], true, 10, 44100);
            Debug.Log("I am using audio from: " + audioDevices[0]);
            audioSource.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();
        }
        else
            return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
