using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//using Button = UnityEngine.UI.Button;

public class DashboardManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private RenderTexture avatarBodyCameraTexture;
    [SerializeField] private RenderTexture avatarFaceCameraTexture;
    [SerializeField] private RawImage avatarBodyWindowImage;
    [SerializeField] private RawImage avatarFaceWindowImage;

    public void StartButtonOnclick()
    {
        startButton.interactable = false;
        AvatarDisplay(avatarBodyWindowImage, avatarBodyCameraTexture, avatarFaceWindowImage, avatarFaceCameraTexture);
    }

    private void AvatarDisplay(RawImage bodyWindow, RenderTexture bodyCameraRenderTexture, RawImage faceWindow, RenderTexture faceCameraRenderTexture)
    {
        VirtualCameraDisplay(bodyWindow, bodyCameraRenderTexture); // bodywindow
        VirtualCameraDisplay(faceWindow, faceCameraRenderTexture); // facewindow
    }

    private void VirtualCameraDisplay(RawImage img, RenderTexture texture)
    {
        if (img != null && texture != null)
        {
            img.texture = texture;
            img.color = Color.white;
        }
    }
}
