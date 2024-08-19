using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject referencePoint;
    public float distanceOffset;
    public float hightOffset;
    public Canvas canvas;
    public enum cameraType { body, head}
    public cameraType type = cameraType.body;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateCamera();
    }

    public void UpdateCamera()
    {
        if (type == cameraType.body)
        {
            var hightVect = new Vector3(0, hightOffset, 0);
            transform.rotation = referencePoint.transform.rotation;
            transform.Rotate(0, 90, 0);
            transform.Rotate(0, 0, 90);
            transform.position = referencePoint.transform.position - referencePoint.transform.right * distanceOffset;
            canvas.transform.rotation = transform.rotation;
            canvas.transform.position = referencePoint.transform.right * distanceOffset - referencePoint.transform.position + hightVect;
        }
        else if(type == cameraType.head)
        {
            var hightVect = new Vector3(0, hightOffset, 0);
            transform.rotation = referencePoint.transform.rotation;
            transform.Rotate(90, 0, 0);
            transform.Rotate(0, 0, 90);
            transform.position = referencePoint.transform.position + referencePoint.transform.up * distanceOffset + hightVect;
        }
    }

    
}
