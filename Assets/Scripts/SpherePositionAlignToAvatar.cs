using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePositionAlignToAvatar : MonoBehaviour
{
    public GameObject avatar;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = new Vector3(avatar.transform.position.x, avatar.transform.position.y, avatar.transform.position.z);
    }
}
