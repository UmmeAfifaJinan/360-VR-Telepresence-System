using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AIAnalysis : MonoBehaviour
{
    public TMP_Text HR, GSR, emotion;

    [HideInInspector]
    public string hr_str, gsr_str, emotion_str;

    // Start is called before the first frame update
    void Start()
    {
        ReadExternalData();
    }

    // Update is called once per frame
    void Update()
    {
        InformationDisplay();
    }

    public void ReadExternalData()
    {
        var temp = "Loading...";
        hr_str = temp;
        gsr_str = temp;
        emotion_str = temp;
    }

    public void InformationDisplay()
    {
        HR.text = "HR: " + hr_str;
        GSR.text = "GSR: " + gsr_str;
        emotion.text = "Emotion: " + emotion_str;
    }
}
