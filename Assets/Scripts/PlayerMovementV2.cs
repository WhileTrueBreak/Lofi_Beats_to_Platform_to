using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementV2 : MonoBehaviour {
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
    
    private float lastGrounded;
    private bool isGrounded = false;
    private bool collidedDown = false;
    private bool collidedRight = false;
    private bool collidedLeft = false;
    private bool collidedUp = false;
    
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
    
    // flags
    private bool allowWalk = true;
    private bool allowAerial = false;
    private bool allowGravity = true;
    private bool allowJump = false;
    private bool allowWallJump = false;
    private bool allowDash = false;
    private bool isJumping = false;
    private bool isJumpStart = false;
    private bool isDashing = false;
    
    // timers
    private float lastDash = -1;
    private float lastJump = -1;
    private float lastOnWall = -1;
    
    void setFlags(){
        resetFlags();
        checkWalkFlag();
        checkAerialFlag();
        checkGravityFlag();
        checkJumpFlag();
        checkWallJumpFlag();
        checkDashFlag();
    }
    
    void resetFlags(){
        allowWalk = true;
        allowAerial = false;
        allowGravity = true;
        allowJump = false;
        allowWallJump = false;
        allowDash = false; 
    }
    
    void checkWalkFlag(){
        if(!isGrounded) allowWalk = false;
    }
    
    void checkAerialFlag(){
        if(!isGrounded) allowAerial = true;
    }
    
    void checkGravityFlag(){
        if(isDashing) allowGravity = false;
        if(isJumpStart) allowGravity = false;
    }
    
    void checkJumpFlag(){
        if(isGrounded) allowJump = true;
        // maybe cancel jump when hitting roof?
        
        // must be jumping to be at teh start of a jump
        if(!isJumping) isJumpStart = false;
    }
    
    void checkWallJumpFlag(){
        // check if on wall and not on ground
        if((collidedLeft||collidedRight)&&!isGrounded) allowWallJump = true;
    }
    
    void checkDashFlag(){
        if(!isGrounded) allowDash = true;
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
