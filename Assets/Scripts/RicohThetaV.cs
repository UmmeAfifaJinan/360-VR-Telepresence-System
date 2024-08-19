using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RicohThetaV : MonoBehaviour
{
	public Material renderMaterial;
	public streamingResolutions streamingRes;
	public enum streamingResolutions { R_FullHD, R_4K, R_UVC }; // Set streaming resolution profiles	

	private string camName;
	private string RICOH_DRIVER_NAME = "";
	private const int THETA_V_AUDIO_NUMBER = 0;
	private AudioSource audioSource;

	void Start()
	{
		ReadSteamingCamera();
		ProcessStreamingAudio();
		ProcessStreamingTexture();
		// InvertSphere();
	}

	private void ReadSteamingCamera()
	{
		// RICOH_DRIVER_NAME = (streamingRes == streamingResolutions.R_FullHD) ? "RICOH THETA V FullHD" : "RICOH THETA V 4K";
		switch (streamingRes)
		{
			case streamingResolutions.R_FullHD: RICOH_DRIVER_NAME = "RICOH THETA V FullHD"; break;
			case streamingResolutions.R_4K: RICOH_DRIVER_NAME = "RICOH THETA V 4K"; break;
			case streamingResolutions.R_UVC: RICOH_DRIVER_NAME = "RICOH THETA UVC"; break; // we found this out thru trial n error in OBS
            default: RICOH_DRIVER_NAME = "RICOH THETA UVC"; break;
		}

        WebCamDevice[] devices = WebCamTexture.devices; // Get devices list
		Debug.Log("Number of web cams connected: " + devices.Length);
		for (int i = 0; i < devices.Length; i++)
		{
			Debug.Log(i + " " + devices[i].name);
			if (devices[i].name == RICOH_DRIVER_NAME)
			{
				camName = devices[i].name;
			}
		}

		Debug.Log($"I am using the webcam named {camName}");

		if (camName != RICOH_DRIVER_NAME)
		{
			Debug.Log("ERROR: " + RICOH_DRIVER_NAME +
				" not found. Install Ricoh streaming driver from https://topics.theta360.com/uk/faq/c_06_v/304_1/. Make sure your camera is in live streaming mode");
		}
	}

	private void ProcessStreamingTexture()
	{
		//Renderer rend = GetComponent<Renderer>();
		WebCamTexture mycam = new WebCamTexture(camName);
		//rend.material.mainTexture = mycam;
		renderMaterial.mainTexture = mycam;  //change the texture of skybox material
		mycam.Play();
	}
	
	private void ProcessStreamingAudio()
	{
		audioSource = GetComponent<AudioSource>();
		string[] audioDevices = Microphone.devices;


		for (int i = 0; i < audioDevices.Length; i++)
		{
			Debug.Log(i + " " + audioDevices[i]);
		}

		audioSource.clip = Microphone.Start(audioDevices[THETA_V_AUDIO_NUMBER], true, 10, 44100);
		Debug.Log("I am using audio from: " + audioDevices[THETA_V_AUDIO_NUMBER]);
		audioSource.loop = true;
		while (!(Microphone.GetPosition(null) > 0)) { }
		audioSource.Play();
	}

    /*This function only apply for sphere 360 solution*/
    private void InvertSphere()
    {
        Vector3[] normals = GetComponent<MeshFilter>().mesh.normals;

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i]; // point the normal from outside to inside
        }

        GetComponent<MeshFilter>().sharedMesh.normals = normals;

        int[] triangles = GetComponent<MeshFilter>().sharedMesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int t = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = t;
        }
		GetComponent<MeshFilter>().sharedMesh.triangles = triangles;


    }
}
