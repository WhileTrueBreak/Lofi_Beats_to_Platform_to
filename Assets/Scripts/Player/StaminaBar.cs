using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour {
    
    [SerializeField] public Sprite emptySprite;
    [SerializeField] public Sprite fullSprite;

    private int maxStamina;
    private int currentStamina;
    
    public void Start(){
        
    }

    public void setValue(int value){
        currentStamina = value;
        updateSprites();
    }
    
    public void setMaxValue(int value){
        maxStamina = value;
        renderSprites();
    }

    private void createCellObjects(){
        /*
        Creates the stamina cells
        */
        for (int i=0;i<maxStamina;i++){
            GameObject cell = new GameObject("S_Cell " + (i+1).ToString(), typeof(SpriteRenderer));
            cell.transform.position = transform.position;
            cell.transform.parent = transform;
            cell.transform.position += Vector3.right * (i*emptySprite.bounds.size.x*1.1f);
            Debug.Log(cell.transform.position);
        }
    }

    private SpriteRenderer[] cells;
    private void renderSprites(){
        /*
        Creates the stamina bars if it already doesnt exist
        */
        if (transform.childCount == 0) createCellObjects();

        // Renders the cells
        cells = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer sr in cells){
            sr.sprite = emptySprite;
        }

    }

    private void updateSprites(){
        /*
        Changes the sprites from full to empty or vice versa
        */

        cells = GetComponentsInChildren<SpriteRenderer>();
        for (int i=0;i<currentStamina;i++){
            // cells[i].sprite = fullSprite;
            cells[i].color = Color.green;
        }
        for (int i=currentStamina;i<maxStamina;i++){
            // cells[i].sprite = emptySprite;
            cells[i].color = Color.white;
            
        }
        
    }
    
}
