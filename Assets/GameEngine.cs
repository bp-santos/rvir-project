using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.IO;
using TMPro;

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

    private void Start()
    {
        _inputData = GetComponent<InputData>();
        clickCount = 0;
        isRunning = false;
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
            }

            if (CheckTargetMaterials())
            {
                Debug.Log("Finished");
                isRunning = false;
                Debug.Log(elapsedTime);
                SaveTimeToCSV();
            }
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
            File.WriteAllText(filePath, "Clicks,Errors,Minutes,Seconds,Milliseconds\n");
        }

        string timeData = $"{clickCount},{clickCount-8},{Mathf.FloorToInt(elapsedTime / 60f)},{Mathf.FloorToInt(elapsedTime % 60f)},{Mathf.FloorToInt((elapsedTime * 1000) % 1000)}\n";
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
