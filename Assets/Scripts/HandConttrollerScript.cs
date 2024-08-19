using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandConttrollerScript : MonoBehaviour
{
    [SerializeField] InputActionReference gripButtonAction;
    [SerializeField] InputActionReference triggerButtonAction;
    private Animator handAnimator;

    private void Awake()
    {
        handAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        gripButtonAction.action.performed += GripButtonPressed;
        triggerButtonAction.action.performed += TriggerButtonPressed;
    }
    private void OnDisable()
    {
        gripButtonAction.action.performed -= GripButtonPressed;
        triggerButtonAction.action.performed -= TriggerButtonPressed;
    }

    private void TriggerButtonPressed(InputAction.CallbackContext obj)
    {
        handAnimator.SetFloat("Trigger", obj.ReadValue<float>());
    }

    private void GripButtonPressed(InputAction.CallbackContext obj)
    {
        handAnimator.SetFloat("Grip", obj.ReadValue<float>());
    }
}
