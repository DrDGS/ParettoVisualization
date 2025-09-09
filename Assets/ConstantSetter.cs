using UnityEngine;
using UnityEngine.UI;

public class ConstantSetter : MonoBehaviour
{
    public float[] a = new float[5];
    public float[] b = new float[5];
    public float[] psi = new float[5];
    public float[] p = new float[5];

    bool running = false;

    SliderController sliderController;
    Dropdown[] dropdowns = new Dropdown[4];
    InputField[] inputFields = new InputField[4];
    Button startButton;

    void Start()
    {
        gameObjectsSetup();
        updateConstants();
    }

    void gameObjectsSetup()
    {
        sliderController = GameObject.Find("Main Camera").GetComponent<SliderController>();
        startButton = GameObject.Find("Button").GetComponent<Button>();
        for (int i = 0; i < 4; i++)
        {
            dropdowns[i] = GameObject.Find("Dropdown" + (i + 1).ToString()).GetComponent<Dropdown>();
            inputFields[i] = GameObject.Find("InputField" + (i + 1).ToString()).GetComponent<InputField>();
        }
    }

    void updateConstants()
    {
        sliderController.pinA.CopyTo(a, 0);
        sliderController.pinB.CopyTo(b, 0);
        sliderController.pinPSI.CopyTo(psi, 0);
        sliderController.pinP.CopyTo (p, 0);
    }
}
