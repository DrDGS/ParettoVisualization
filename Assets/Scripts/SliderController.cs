using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SliderController : MonoBehaviour
{
    private Transform selectedPin = null;
    private Camera mainCamera;

    private float[] pinG = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }; //G values of 
    private float[] pinA = { 2f, 1f, 3f, 1f, 1f };
    private float[] pinB = { 2f, 2f, 2f, 2f, 2f };
    private float[] pinPSI = { 0.3f, 0.1f, 0.1f, 0.4f, 0.1f };

    private float[] pinP = { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };
    private int iterations = 0;

    [SerializeField] Material baseMaterial;
    [SerializeField] Material outMaterial;

    float mainFunc()
    {
        float result = 0f;
        for (int i = 0; i < 5; i++)
        {
            result += pinA[i] * MathF.Pow(pinG[i], pinB[i]);
        }
        return result;
    }

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        simulationStart();
    }

    void simulationStart()
    {
        setPinSizes();
        if (getFirstPoint()) //todo - add loading
            setSlidersPosition();
        else print("No Paretto-efficient points were found");
    }

    void setPinSizes() //height = PSI/2
    {
        for (int i = 0; i < 5; i++)
        {
            Transform currentPin = GameObject.Find(pinNumtoName(i)).GetComponent<Transform>();
            currentPin.localScale = new Vector3(currentPin.localScale.x, pinPSI[i]*1.2f, currentPin.localScale.z);
        }
    }

    bool getFirstPoint()
    {
        pinP.CopyTo(pinG, 0);
        if (mainFunc() > 1) return false;
        moveLogic(-1, -1);
        return true;
    }

    void setSlidersPosition() //sets localPositions according to pinG values
    {
        for (int pinNum = 0; pinNum < 5; pinNum++)
        {
            Transform currentPin = GameObject.Find(pinNumtoName(pinNum)).GetComponent<Transform>();
            currentPin.localPosition = GValueToPosition(pinNum, pinG[pinNum]);
        }
    }

    void Update()
    {
        Vector3 oldestPosition = Vector3.zero;
        // Начало перетаскивания
        if (Input.GetMouseButtonDown(0) && selectedPin == null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name.StartsWith("Pin"))
                {
                    selectedPin = hit.transform;
                }
            }
        }
        int pinNum = 0;

        // Перетаскивание
        if (Input.GetMouseButton(0) && selectedPin != null)
        {
            pinNum = selectedPin.name[3] - '1';
            float currentMouseY = (Input.mousePosition.y/Screen.height)*2;
            Vector3 newPosition = new Vector3(
                Lerp(selectedPin.localPosition.x, currentMouseY-1, 0.01f), 
                Lerp(selectedPin.localPosition.y, currentMouseY, 0.01f), 
                selectedPin.localPosition.z
                );

            if (Mathf.Abs(newPosition.x) <= 0.7) //checking if mouse is dragging pin out of [0,1] boundaries
            {
                Vector3 oldPosition = selectedPin.localPosition;
                double deltaG = positionToGValue(newPosition) - positionToGValue(oldPosition);
                
                if ((deltaG > 0 && checkBoundariesDown(pinNum)) || (deltaG < 0 && checkBoundariesUp(pinNum)))
                {
                    pinG[pinNum] = positionToGValue(newPosition);
                    moveLogic(pinNum, (float)deltaG);
                    setSlidersPosition();
                    print("!!!");
                    //checkAndDye();
                }
            }
        }

        // Конец перетаскивания
        if (Input.GetMouseButtonUp(0))
        {
            selectedPin = null;
        }
    }

    void moveLogic(int pinNum, float delta)
    {
        float[] k = new float[6];
        for (int i = 0; i < 5; i++)
        {
            if (i == pinNum)
            {
                k[i] = 0;
                continue;
            }

            k[i] = (1 + pinPSI[i] - ((pinNum != -1 ? 1 - pinPSI[pinNum] : 1) / 4)) * MathF.Pow(1 / pinA[i], 1 / pinB[i]);
            k[i] = delta > 0 ? -1 / k[i] : k[i];
        }

        while (MathF.Abs(mainFunc() - 1) > 0.01)
        {
            adjustG(k);
        }
        roundG();
        print("!!");
    }

    void adjustG(float[] k)
    {
        for(int i = 0; i < 5; i++)
            if (pinG[i] < 1 && pinG[i] > 0)
            pinG[i] += 0.00005f * k[i];
    }

    void checkAndDye()
    {
        for (int i = 0;i < 5; i++)
        {
            if (pinG[i] < pinP[i])
                GameObject.Find(pinNumtoName(i)).GetComponent<Renderer>().material = outMaterial;
            else GameObject.Find(pinNumtoName(i)).GetComponent<Renderer>().material = baseMaterial;
        }
    }

    void roundG()
    {
        for (int i = 0; i < 5; i++)
            pinG[i] = MathF.Floor(pinG[i] * 10000) / 10000;
    }
    
    bool checkBoundariesUp(int pinNum) //check if there is at least 1 pin not fully up (excluding selected pin)
    {
        for (int i = 0; i < 5; i++)
        {
            if (pinG[i] < 1f && i != pinNum)
                return true;
        }
        return false;
    }

    bool checkBoundariesDown(int pinNum) //check if there is at least 1 pin not fully down (excluding selected pin)
    {
        for (int i = 0; i < 5; i++)
        {
            if (pinG[i] > 0f && i != pinNum)
                return true;
        }
        return false;
    }


    //---Utility functions---
    float positionToGValue(Vector3 position)
    {
        return MathF.Round((float)((position.x + 0.7) / 1.4), 4);
    }

    public static float Lerp(float current, float target, float speed)
    {
        return current + (target - current) * Math.Clamp(speed, 0f, 1f);
    }

    Vector3 GValueToPosition(int pinNum, float G)
    {
        return new Vector3(
            (float)(G * 1.4 - 0.7),
            (float)(G * 1.4 + 0.3),
            (float)(1 - pinNum * 0.6));
    }

    String pinNumtoName(int pinNum)
    {
        return "Pin" + (pinNum+1).ToString();
    }

    int NametoPinNum(String name)
    {
        return name[3] - '1';
    }
}
