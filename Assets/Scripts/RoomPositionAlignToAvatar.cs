using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPositionAlignToAvatar : MonoBehaviour
{
    public GameObject avatarCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = new Vector3(avatarCamera.transform.position.x + (float)3.5, avatarCamera.transform.position.y - (float)1.67, avatarCamera.transform.position.z + (float)2.2);
    }
}
