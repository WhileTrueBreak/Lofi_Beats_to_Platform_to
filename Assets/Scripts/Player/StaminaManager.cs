using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaManager : MonoBehaviour {
    
    [SerializeField] public StaminaBar staminaBar;
    
    private int currentStamina=0;
    
    [SerializeField] public int maxStamina;
    
    [SerializeField] public int dashCost;
    [SerializeField] public int wallJumpCost;
    [SerializeField] public int onBeatGain;
    [SerializeField] public int passiveDecayRate;

    private bool staminaFlag;
    private int missedBeats;
    
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
            missedBeats++;
            staminaFlag = false;
        }else{
            missedBeats = 0;
            staminaFlag = false;
        }

        if (missedBeats >= passiveDecayRate){
            removeStamina(1);
            staminaFlag = false;
            missedBeats = 0;
        }
    }
}
