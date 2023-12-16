using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(InputData))]
public class GameEngine : MonoBehaviour
{
    private InputData _inputData;
    private float elapsedTime;
    private bool isRunning;
    private int clickCount;
    private bool leftGripPressed;
    private bool rightGripPressed;

    public GameObject[] targetObjects;
    public Material[] materials;
    public Material selectedMaterial;
    public TextMeshPro timeText;
    public int selectionTechnique;

    private void Start()
    {
        _inputData = GetComponent<InputData>();
        clickCount = 0;
        isRunning = false;
        SetRays();
        UpdateTimeText();
    }

    void Update()
    {
        bool leftButtonState;
        bool rightButtonState;

        if (_inputData._leftController.TryGetFeatureValue(CommonUsages.triggerButton, out leftButtonState) && 
            _inputData._rightController.TryGetFeatureValue(CommonUsages.triggerButton, out rightButtonState))
        {
            if (rightButtonState && leftButtonState)
            {
                restartMaterials();
                elapsedTime = 0f;
                clickCount = 0;
                Debug.Log("Started");
                isRunning = true;
                leftGripPressed = false;
                rightGripPressed = false;
            }
        }

        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimeText();

            if (_inputData._leftController.TryGetFeatureValue(CommonUsages.gripButton, out leftButtonState) && 
                _inputData._rightController.TryGetFeatureValue(CommonUsages.gripButton, out rightButtonState))
            {
                if (rightButtonState && !rightGripPressed)
                {
                    clickCount++;
                    rightGripPressed = true;
                }

                if (leftButtonState && !leftGripPressed)
                {
                    clickCount++;
                    leftGripPressed = true;
                }

                if (!rightButtonState)
                {
                    rightGripPressed = false;
                }

                if (!leftButtonState)
                {
                    leftGripPressed = false;
                }

                if (rightButtonState && leftButtonState)
                {   
                    GameObject obj1 = null;
                    GameObject obj2 = null;

                    if(selectionTechnique == 1)
                    {
                        obj1 = GetObject(_inputData._leftController,"_image");
                        obj2 = GetObject(_inputData._rightController,"_frame");
                    }

                    if(selectionTechnique == 2){
                        obj1 = GetObject(_inputData._cameraRay, "_image");
                        obj2 = GetObject(_inputData._rightController, "_frame");
                    }

                    if(obj1 != null && obj2 != null && obj1 != obj2){
                        Bounds bounds1 = obj1.GetComponent<Renderer>().bounds;
                        Bounds bounds2 = obj2.GetComponent<Renderer>().bounds;

                        if (Vector3.Distance(bounds1.size, bounds2.size) < 0.000001)
                        {
                            obj1.GetComponent<Renderer>().material = selectedMaterial;
                            obj2.GetComponent<Renderer>().material = selectedMaterial;
                        }
                    }
                }
            }

            if (CheckTargetMaterials())
            {
                Debug.Log("Finished");
                isRunning = false;
                SaveTimeToCSV();
            }
        }
    }

    private void SetRays()
    {
        switch (selectionTechnique)
        {
            case 0:
                SetInteractorActive(false, "_leftController");
                SetInteractorActive(true, "_rightController");
                SetInteractorActive(false, "_ray");
                break;
            case 1:
                SetInteractorActive(true, "_leftController");
                SetInteractorActive(true, "_rightController");
                SetInteractorActive(false, "_ray");
                break;
            case 2:
                SetInteractorActive(false, "_leftController");
                SetInteractorActive(true, "_rightController");
                SetInteractorActive(true, "_ray");
                break;
            default:
                break;
        }
    }

    private void SetInteractorActive(bool isActive, string tag)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
        {
            XRBaseInteractor interactor = obj.GetComponent<XRBaseInteractor>();
            if (interactor != null)
            {
                interactor.enabled = isActive;
            }
        }
    }

    private GameObject GetObject(InputDevice controller, string tag)
    {
        if (controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 controllerPosition) &&
            controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion controllerRotation))
        {

            RaycastHit hit;
            Ray ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);

            //if(selectionTechnique == 1){
            //    ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);
            //}
            if(selectionTechnique == 2){
                Vector3 adjustedPosition = new Vector3(controllerPosition.x, controllerPosition.y + 0.2f, controllerPosition.z);
                ray = new Ray(adjustedPosition, controllerRotation * Vector3.forward);
            }

            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = hit.collider.gameObject;
                if(obj.CompareTag(tag))
                {
                    return obj;
                }
            }
        }

        return null;
    }

    public string GetInteractionTechniqueName()
    {
        switch(selectionTechnique){
            case 0:
                return "One hand";
            case 1:
                return "Two hands";
            case 2:
                return "DH and HMD";
            default:
                return "NaN";
        }
    }


    private void restartMaterials()
    {
        for (int i = 0; i <= 7; i++)
        {
            targetObjects[i].GetComponent<Renderer>().material = materials[i];
        }
    }

    private bool CheckTargetMaterials()
    {
        foreach (GameObject targetObject in targetObjects)
        {
            Material material = targetObject.GetComponent<Renderer>().material;
            if (material.shader != selectedMaterial.shader)
            {
                return false;
            }
        }
        return true;
    }

    private void SaveTimeToCSV()
    {
        string filePath = "Assets/TimeData.csv";

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Technique,Clicks,Errors,Minutes,Seconds,Milliseconds\n");
        }

        string timeData = $"{GetInteractionTechniqueName()},{clickCount},{clickCount-8},{Mathf.FloorToInt(elapsedTime / 60f)},{Mathf.FloorToInt(elapsedTime % 60f)},{Mathf.FloorToInt((elapsedTime * 1000) % 1000)}\n";
        File.AppendAllText(filePath, timeData);
    }

    private void UpdateTimeText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000) % 1000);

        timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}
