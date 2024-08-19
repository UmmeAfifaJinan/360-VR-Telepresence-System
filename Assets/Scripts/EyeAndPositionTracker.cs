using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EyeAndPositionTracker : MonoBehaviour
{
    public GameObject REye;
    public GameObject LEye;
    public GameObject RHand;
    public GameObject LHand;
    public GameObject Head;

    private string CSVPath;
    private bool dataStarted;

    // Start is called before the first frame update
    void Start()
    {
        CSVPath = string.Concat(DateTime.Now.ToString("yyyy-MM-dd-HH"), "_Avatar_Controllers_and_Head_Data", ".csv");
        CSVPath = string.Concat(Application.streamingAssetsPath, "/", CSVPath);
    }

    // Update is called once per frame
    void Update()
    {
        using (StreamWriter f = File.AppendText(CSVPath))
        {
            if (!dataStarted)
            {
                f.WriteLine("REye_Position_X, REye_Position_Y, REye_Position_Z, REye_Rotation_X, REye_Rotation_Y, REye_Rotation_Z" +
                            "LEye_Position_X, LEye_Position_Y, LEye_Position_Z, LEye_Rotation_X, LEye_Rotation_Y, LEye_Rotation_Z" +
                            "RHand_Position_X, RHand_Position_Y, RHand_Position_Z, RHand_Rotation_X, RHand_Rotation_Y, RHand_Rotation_Z" +
                            "LHand_Position_X, LHand_Position_Y, LHand_Position_Z, LHand_Rotation_X, LHand_Rotation_Y, LHand_Rotation_Z" +
                            "Head_Position_X, Head_Position_Y, Head_Position_Z, Head_Rotation_X, Head_Rotation_Y, Head_Rotation_Z");
                dataStarted = true;
            }
            f.WriteLine(REye.transform.position.x + "," + REye.transform.position.y + ", " + REye.transform.position.z + "," +
                        REye.transform.rotation.x + "," + REye.transform.rotation.y + ", " + REye.transform.rotation.z + "," +

                        LEye.transform.position.x + "," + LEye.transform.position.y + ", " + LEye.transform.position.z + "," +
                        LEye.transform.rotation.x + "," + LEye.transform.rotation.y + ", " + LEye.transform.rotation.z + "," +

                        RHand.transform.position.x + "," + RHand.transform.position.y + ", " + RHand.transform.position.z + "," +
                        RHand.transform.rotation.x + "," + RHand.transform.rotation.y + ", " + RHand.transform.rotation.z + "," +

                        LHand.transform.position.x + "," + LHand.transform.position.y + ", " + LHand.transform.position.z + "," +
                        LHand.transform.rotation.x + "," + LHand.transform.rotation.y + ", " + LHand.transform.rotation.z + "," +

                        Head.transform.position.x + "," + Head.transform.position.y + ", " + Head.transform.position.z + "," +
                        Head.transform.rotation.x + "," + Head.transform.rotation.y + ", " + Head.transform.rotation.z + ",");
        }

    }
}
