using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerScript : MonoBehaviour
{
    [SerializeField] InputActionReference gripInputAction;
    [SerializeField] InputActionReference triggerInputAction;
    [SerializeField] InputActionReference joyStickInputAction;

    Animator controllerAnimator;

    private void Awake()
    {
        controllerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        gripInputAction.action.performed += GripPressed;
        triggerInputAction.action.performed += TriggerPressed;
        joyStickInputAction.action.performed += JoyStickPressed;
    }

    private void OnDisable()
    {
        gripInputAction.action.performed -= GripPressed;
        triggerInputAction.action.performed -= TriggerPressed;
        joyStickInputAction.action.performed -= JoyStickPressed;
    }
    private void GripPressed(InputAction.CallbackContext obj)
    {
        controllerAnimator.SetFloat("Grip", obj.ReadValue<float>()); //Read value from input
    }
    private void TriggerPressed(InputAction.CallbackContext obj)
    {
        controllerAnimator.SetFloat("Trigger", obj.ReadValue<float>());
    }
    private void JoyStickPressed(InputAction.CallbackContext obj)
    {
        controllerAnimator.SetFloat("Joy X", obj.ReadValue<Vector2>().x);
        controllerAnimator.SetFloat("Joy Y", obj.ReadValue<Vector2>().y);
    }
}
