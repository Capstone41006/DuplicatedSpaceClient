using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OculusFunctions : MonoBehaviour
{
    // Controller
    [SerializeField] InputActionReference controllerActionJoyLeft;
    [SerializeField] InputActionReference controllerActionJoyRight;
    [SerializeField] InputActionReference controllerActionBtnLT;
    [SerializeField] InputActionReference controllerActionBtnRT;
    [SerializeField] InputActionReference controllerActionBtnLB;
    [SerializeField] InputActionReference controllerActionBtnRB;
    [SerializeField] InputActionReference controllerActionBtnA;  // rightcontroller A
    [SerializeField] InputActionReference controllerActionBtnB;  // rightcontroller B
    //[SerializeField] InputActionReference controllerActionBtnX;  // leftcontroller X
    //[SerializeField] InputActionReference controllerActionBtnY;  // leftcontroller Y

    [HideInInspector] public float axisLeftX;
    [HideInInspector] public float axisLeftY;
    [HideInInspector] public float axisRightX;
    [HideInInspector] public float axisRightY;
    [HideInInspector] public float buttonLT;
    [HideInInspector] public float buttonRT;

    [HideInInspector] public bool buttonLB;
    [HideInInspector] public bool buttonRB;
    [HideInInspector] public bool buttonA;
    [HideInInspector] public bool buttonB;
    [HideInInspector] public bool buttonX;
    [HideInInspector] public bool buttonY;
    [HideInInspector] public bool buttonX_c;
    [HideInInspector] public bool buttonY_c;


    private void Awake()
    {
        controllerActionJoyLeft.action.performed += JoyStickLeft;
        controllerActionJoyRight.action.performed += JoyStickRight;
        controllerActionBtnLT.action.performed += ControllerLT;
        controllerActionBtnRT.action.performed += ControllerRT;
        controllerActionBtnLB.action.performed += ControllerLB;
        controllerActionBtnRB.action.performed += ControllerRB;

        controllerActionBtnA.action.performed += ControllerA;
        controllerActionBtnB.action.performed += ControllerB;
        //controllerActionBtnX.action.performed += ControllerX;
        //controllerActionBtnY.action.performed += ControllerY;
        //controllerActionBtnX.action.canceled += ControllerX_c;      //
        //controllerActionBtnY.action.canceled += ControllerY_c;        //
    }


    public void JoyStickLeft(InputAction.CallbackContext obj)
    {
        Vector2 tempVector2 = obj.ReadValue<Vector2>();
        axisLeftX = tempVector2[0];
        axisLeftY = -tempVector2[1];
        //Debug.Log($"axisLeftX: {axisLeftX}");
        //Debug.Log($"axisLeftY: {axisLeftY}");
    }

    public void JoyStickRight(InputAction.CallbackContext obj)
    {
        Vector2 tempVector2 = obj.ReadValue<Vector2>();
        axisRightX = tempVector2[0];
        axisRightY = tempVector2[1];
        //Debug.Log($"axisRightX: {axisRightX}");
    }

    public void ControllerLT(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        buttonLT = temp;
        //Debug.Log($"buttonLT: {buttonLT}");
    }

    public void ControllerRT(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        buttonRT = temp;
        //Debug.Log($"buttonRT: {buttonRT}");
    }

    public void ControllerLB(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp >= 0.5f)
        {
            buttonLB = true;
        }
        else
        {
            buttonLB = false;
        }
        //Debug.Log($"buttonLB: {buttonLB}");
    }

    public void ControllerRB(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp >= 0.5f)
        {
            buttonRB = true;
        }
        else
        {
            buttonRB = false;
        }
        //Debug.Log($"buttonRB: {buttonRB}");
    }


    public void ControllerA(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp == 1)
        {
            buttonA = true;
        }
        else
        {
            buttonA = false;
        }
        //Debug.Log($"buttonA: {buttonA}");
    }

    public void ControllerB(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp == 1)
        {
            buttonB = true;
        }
        else
        {
            buttonB = false;
        }
        //Debug.Log($"buttonB: {buttonB}");
    }

    public void ControllerX(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp == 1)
        {
            buttonX = true;
        }
        else
        {
            //buttonX = false;
        }
        //Debug.Log($"buttonX: {buttonX}");
    }

    public void ControllerY(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp == 1)
        {
            buttonY = true;
        }
        else
        {
            //buttonY = false;
        }
        //Debug.Log($"buttonY: {buttonY}");
    }

    public void ControllerX_c(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp == 1)
        {
            //buttonX_c = true;
        }
        else
        {
            buttonX = false;
        }
        //Debug.Log($"buttonX: {buttonX}");
    }

    public void ControllerY_c(InputAction.CallbackContext obj)
    {
        float temp = obj.ReadValue<float>();
        if (temp == 1)
        {
            //buttonY_c = true;
        }
        else
        {
            buttonY = false;
        }
        //Debug.Log($"buttonY_c: {buttonY_c}");
    }
}
