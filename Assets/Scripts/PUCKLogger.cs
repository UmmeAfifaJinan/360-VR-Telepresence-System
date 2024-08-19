using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PUCKLogger : MonoBehaviour
{

    private string CSVPath;
    public GameObject trackerCube;
    private bool dataStarted;


    // Start is called before the first frame update
    void Start()
    {
        CSVPath = string.Concat(DateTime.Now.ToString("yyyy-MM-dd-HH"), "_Tracker_Data", ".csv");
        CSVPath = string.Concat(Application.streamingAssetsPath, "/", CSVPath);
    }

    // Update is called once per frame
    void Update()
    {
        using (StreamWriter f = File.AppendText(CSVPath))
        {
            if (!dataStarted)
            {
                f.WriteLine("Position_X, Position_Y, Position_Z, Rotation_X, Rotation_Y, Rotation_Z");
                dataStarted = true;
            }
            f.WriteLine(trackerCube.transform.position.x + "," + trackerCube.transform.position.y + ", " + trackerCube.transform.position.z + "," +
                        trackerCube.transform.rotation.x + "," + trackerCube.transform.rotation.y + ", " + trackerCube.transform.rotation.z + ",");
        }
    }
}
