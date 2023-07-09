using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaManager : MonoBehaviour {
    
    public StaminaBar staminaBar;
    
    public int currentStamina;
    
    public int maxStamina;
    
    public int onBeatStamina;
    public int dashCost;
    public int wallJumpCost;
    
    // Start is called before the first frame update
    void Start() {
        staminaBar.setMaxValue(maxStamina);
        staminaBar.setValue(currentStamina);
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)){
            currentStamina += 1;
            staminaBar.setValue(currentStamina);
        }
        if(Input.GetKeyDown(KeyCode.LeftAlt)){
            currentStamina -= 1;
            staminaBar.setValue(currentStamina);
        }
    }
    
    
    
}
