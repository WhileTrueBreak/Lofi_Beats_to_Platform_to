using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaManager : MonoBehaviour {
    
    public StaminaBar staminaBar;
    
    private int currentStamina=0;
    
    public int maxStamina;
    
    public int dashCost;
    public int wallJumpCost;
    public int onBeatGain;

    private bool staminaFlag;
    
    // Start is called before the first frame update
    void Start() {
        staminaBar.setMaxValue(maxStamina);
        staminaBar.setValue(currentStamina);
    }

    // Update is called once per frame
    void Update() {
        // staminaBar.setValue(currentStamina);
    }

    public void addStamina(int value){
        staminaFlag = true;
        if (currentStamina < maxStamina){
            currentStamina += value;
        }
        staminaBar.setValue(currentStamina);
    }

    public void removeStamina(int value){
        staminaFlag = true;
        if (currentStamina > 0){
            currentStamina -= value;
        }
        staminaBar.setValue(currentStamina);
    }

    public int getStamina(){
        return currentStamina;
    }

    public void passiveLoss(){
        if (!staminaFlag){
            removeStamina(1);
        }else{
            staminaFlag = false;
        }
    }
}
