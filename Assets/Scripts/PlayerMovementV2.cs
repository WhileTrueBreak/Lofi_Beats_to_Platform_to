using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementV2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        getInputs();
        checkCollisions();
        setFlags();
        calcMovement();
    }
    
    private bool inputLeft = false;
    private bool inputRight = false;
    private bool inputUp = false;
    private bool inputDown = false;
    private bool inputJump = false;
    private bool inputDash = false;
    private float lastJumpInput = -1f;
    private float lastDashInput = -1f;
    
    void getInputs(){
        // get key input
        inputLeft = Input.GetKey("left");
        inputRight = Input.GetKey("right");
        inputUp = Input.GetKey("up");
        inputDown = Input.GetKey("down");
        inputJump = Input.GetKeyDown("x");
        inputDash = Input.GetKeyDown("c");
        
        if(inputJump) lastJumpInput = Time.time;
        if(inputDash) lastDashInput = Time.time;
    }
    
    [SerializeField] private LayerMask ground;
    [SerializeField] private float collidedCheckRange;
    [SerializeField] private int rayCount;
    
    void checkCollisions(){
        Vector2 min = playerBounds.center+transform.position-playerBounds.size/2;
        Vector2 max = playerBounds.center+transform.position+playerBounds.size/2;
        collidedUp = castRays(Vector2.up,       new Vector2(min.x, max.y), max);
        collidedDown = castRays(Vector2.down,   min, new Vector2(max.x, min.y));
        collidedRight = castRays(Vector2.right, new Vector2(max.x, min.y), max);
        collidedLeft = castRays(Vector2.left,   min, new Vector2(min.x, max.y));
        
        // check if just left ground
        if(isGrounded && !collidedDown) lastGrounded = Time.time;
        if(collidedDown && !isGrounded) allowCoyote = true;
        
        isGrounded = collidedDown;
    }
    
    bool castRays(Vector2 dir, Vector2 start, Vector2 end){
        for(int i = 0;i < rayCount;i++){
            Vector2 startPos = Vector2.Lerp(start, end, (float)i/(rayCount-1));
            bool hit = Physics2D.Raycast(startPos, dir, collidedCheckRange, ground);
            if(hit) return true;
        }
        return false;
    }
    
    private bool allowWalk = true;
    private bool allowAerial = false;
    private bool allowGravity = true;
    private bool allowJump = false;
    private bool allowWallJump = false;
    private bool allowDash = false; 
    private bool isJumping = false;
    private bool isDashing = false;
    private bool 
    
    void setFlags(){
        
    }
    
    void calcMovement(){
        calcWalk();
        calcAerialControl();
        calcGravity();
        calcJump();
        calcWallJump();
        calcDash();
    }
    
    void calcWalk(){
        
    }
    
    void calcAerialControl(){
        
    }
    
    void calcGravity(){
        
    }
    
    void calcJump(){
        
    }
    
    void calcWallJump(){
        
    }
    
    void calcDash(){
        
    }
    
    
    
}
