using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(InputData))]
public class MatchImages : MonoBehaviour
{
    private InputData _inputData;
    public GameObject image;
    public GameObject frame;
    public Material lockedMaterial;

    private void Start()
    {
        _inputData = GetComponent<InputData>();
    }

    void Update()
    {
        bool leftButtonState;
        bool rightButtonState;

        if (_inputData._leftController.TryGetFeatureValue(CommonUsages.gripButton, out leftButtonState) && 
        _inputData._rightController.TryGetFeatureValue(CommonUsages.gripButton, out rightButtonState))
        {
            if (rightButtonState && leftButtonState && IsImageSelected(_inputData._leftController) && IsFrameSelected(_inputData._rightController))
            {
                image.GetComponent<Renderer>().material = lockedMaterial;
                frame.GetComponent<Renderer>().material = lockedMaterial;
            }
        }
    }

    private bool IsImageSelected(InputDevice controller)
    {
        if (controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 controllerPosition) &&
            controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion controllerRotation))
        {
            RaycastHit hit;
            Ray ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);

            if (Physics.Raycast(ray, out hit))
            {
                return hit.collider.gameObject == image;
            }
        }

        return false;
    }

    private bool IsFrameSelected(InputDevice controller)
    {
        if (controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 controllerPosition) &&
            controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion controllerRotation))
        {
            RaycastHit hit;
            Ray ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);

            if (Physics.Raycast(ray, out hit))
            {
                return hit.collider.gameObject == frame;
            }
        }

        return false;
    }
}
