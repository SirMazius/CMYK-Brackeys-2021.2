using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public Slider slider;

    private void Awake()
    {   
        slider.value = 0;
    }
    public void InitBar(float max)
    {
        slider.maxValue = max;
    }

    //Añadimos o quitamos en la barra y devolvemos si se ha vaciado
    public bool UpdateValue(float value)
    {
        slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);

        if (slider.value == slider.maxValue)
            return true;
        else
            return false;
    }

    public float GetValue()
    {
        return slider.value;
    }
}
