using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour {
    
    public Slider slider;
    
    public void setValue(int value){
        slider.value = value;
    }
    
    public void setMaxValue(int value){
        slider.maxValue = value;
    }
    
}
