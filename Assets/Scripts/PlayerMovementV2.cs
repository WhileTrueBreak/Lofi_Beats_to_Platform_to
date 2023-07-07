using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementV2 : MonoBehaviour {
    #region variables
    public Vector3 vel;
    
    //other variables
    public float currentGravity;
    #endregion
    
    // Start is called before the first frame update
    void Start() {
        vel = new Vector3(0,0,0);
        currentGravity = gravity;
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
    private float lastMoveInput = -1f;
    private float lastStopInput = -1f;
    
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
        if(inputLeft || inputRight) lastMoveInput = Time.time;
        else lastStopInput = Time.time;
    }
    
    #region collision
    [SerializeField] private Bounds playerBounds;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float collidedCheckRange;
    [SerializeField] private int rayCount;
    #endregion
    
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
        
        // Debug.Log("up:" + collidedUp);
        // Debug.Log("down:" + collidedDown);
        // Debug.Log("left:" + collidedLeft);
        // Debug.Log("right:" + collidedRight);
        
        isGrounded = collidedDown;
        
        if(isGrounded) lastGrounded = Time.time;
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
    private bool allowCoyote = false;
    private bool isJumping = false;
    private bool isJumpStart = false;
    private bool isDashing = false;
    
    // timers
    private float lastDash = -1;
    private float lastJump = -1;
    private float lastOnWall = -1;
    
    void setFlags(){
        resetFlags();
        checkJumpFlag();
        checkWallJumpFlag();
        checkDashFlag();
        checkWalkFlag();
        checkAerialFlag();
        checkGravityFlag();
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
        if(isGrounded) {
            allowCoyote = true;
            allowJump = true;
            isJumping = false;
        }
        // check for coyote time
        if(Time.time-lastGrounded < coyoteBuffer && allowCoyote) allowJump = true;
        
        if(isDashing) isJumping = false;
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
        if(collidedLeft || collidedRight || collidedDown || collidedUp) {
            isDashing = false;
            lastDash = -1;
        }
    }
    
    // walk params
    #region walk
    public float maxWalkSpeed;
    public float walkDecelTime;
    public float walkAccelTime;
    #endregion
    // aerial params
    #region aerial
    public float maxAerialSpeed;
    public float aerialDecelTime;
    public float aerialAccelTime;
    #endregion
    // gravity params
    #region gravity
    public float gravity;
    public float maxFallSpeed;
    #endregion
    // jump params
    #region jump
    public float jumpInputBuffer;
    public float coyoteBuffer;
    public float jumpForce;
    public float apexRange;
    public float apexGravity;
    #endregion
    // wall jump params
    #region wall jump
    
    #endregion
    // dash params
    #region dash
    public float dashForce;
    public float dashDuration;
    public float dashEndSpeed;
    #endregion
    
    void calcMovement(){
        calcWalk();
        calcAerialControl();
        calcJump();
        calcWallJump();
        calcJumpAirTime();
        calcDash();
        calcDashTime();
        calcGravity();
        cancelVel();
        calcNextPosition();
    }
    
    void calcWalk(){
        if(!allowWalk) return;
        var hozVel = vel.x;
        // opposite to input
        // if both inputs are active
        // if no inputs are active
        var isMoveOpposite = ((System.Math.Sign(hozVel) == 1) && inputLeft) || ((System.Math.Sign(hozVel) == -1) && inputRight);
        if (!inputLeft ^ inputRight || isMoveOpposite) {
            hozVel = Mathf.MoveTowards(hozVel, 0, maxWalkSpeed/walkDecelTime*Time.deltaTime);
        }
        
        else if(inputLeft)  hozVel = Mathf.MoveTowards(hozVel, -maxWalkSpeed, maxWalkSpeed/walkAccelTime*Time.deltaTime);
        else if(inputRight) hozVel = Mathf.MoveTowards(hozVel,  maxWalkSpeed, maxWalkSpeed/walkAccelTime*Time.deltaTime);
        
        vel.x = hozVel;
    }
    
    void calcAerialControl(){
        if(!allowAerial) return;
        var hozVel = vel.x;
        // opposite to input
        // if both inputs are active
        // if no inputs are active
        var isMoveOpposite = ((System.Math.Sign(hozVel) == 1) && inputLeft) || ((System.Math.Sign(hozVel) == -1) && inputRight);
        if (!inputLeft ^ inputRight || isMoveOpposite) {
            hozVel = Mathf.MoveTowards(hozVel, 0, maxAerialSpeed/aerialDecelTime*Time.deltaTime);
        }
        
        else if(inputLeft)  hozVel = Mathf.MoveTowards(hozVel, -maxAerialSpeed, maxAerialSpeed/aerialAccelTime*Time.deltaTime);
        else if(inputRight) hozVel = Mathf.MoveTowards(hozVel,  maxAerialSpeed, maxAerialSpeed/aerialAccelTime*Time.deltaTime);
        
        vel.x = hozVel;
    }
    
    void calcGravity(){
        if(!allowGravity) return;
        if(isGrounded) return;
        vel.y -= currentGravity*Time.deltaTime;
        vel.y = Mathf.Max(vel.y, -maxFallSpeed);
    }
    
    void calcJump(){
        if(!allowJump) return;
        // check for last jump input
        if(Time.time-lastJumpInput > jumpInputBuffer) return;
        
        lastJumpInput = -1;
        allowCoyote = false;
        isJumping = true;
        vel.y = jumpForce;
    }
    
    void calcJumpAirTime(){
        if(!isJumping) {
            currentGravity = gravity;
            return;
        }
        float dist = Mathf.InverseLerp(apexRange, 0, Mathf.Abs(vel.y));
        currentGravity = Mathf.Lerp(gravity, apexGravity, dist);
    }
    
    void calcWallJump(){
        if(!allowWallJump) return;
        
    }
    
    void calcDash(){
        if(!allowDash) return;
        if(!inputDash) return;
        //do dash
        //get dash direction
        Vector3 dashVec = new Vector3(0,0,0);
        if(inputLeft) dashVec += Vector3.left;
        if(inputRight) dashVec += Vector3.right;
        if(inputDown) dashVec += Vector3.down;
        if(inputUp) dashVec += Vector3.up;
        dashVec.Normalize();
        if(dashVec.magnitude < 0.1) return;
        
        lastDash = Time.time;
        isDashing = true;
        vel = dashVec*dashForce;
    }
    
    void calcDashTime(){
        if(!isDashing) return;
        if(Time.time-lastDash > dashDuration) isDashing=false;
        var t = (Time.time-lastDash)/dashDuration;
        var mag = Mathf.Lerp(dashForce, dashEndSpeed, t);
        vel.Normalize();
        vel *= mag;
    }
    
    private int maxIterationSteps = 10;
    
    void calcNextPosition(){
        if(vel.magnitude == 0) return;
        
        var moveStep = vel*Time.deltaTime;
        var pos = playerBounds.center+transform.position;
        var endPos = pos + moveStep;
        var hasCollision = Physics2D.OverlapBox(endPos, playerBounds.size, 0, ground);
        //if no collision just move
        if(hasCollision == null){
            transform.position += moveStep;
            return;
        }
        //use binary search to find the farthest point without collision
        var lastValid = 0f;
        var currentPercentage = 0f;
        var nextStep = 0.5f;
        for(var i = 0;i < maxIterationSteps;i++){
            var posToTest = Vector3.Lerp(pos, endPos, currentPercentage);
            var collided = Physics2D.OverlapBox(posToTest, playerBounds.size, 0, ground);
            if(collided) currentPercentage -= nextStep;
            else{
                lastValid = currentPercentage;
                currentPercentage += nextStep;
            }
            nextStep /= 2;
        }
        transform.position = Vector3.Lerp(transform.position, transform.position+moveStep, Mathf.Floor(currentPercentage*10f)/10f);
    }
    
    void cancelVel(){
        if(collidedRight && vel.x > 0) {
            vel.x = 0;
        }
        if(collidedLeft && vel.x < 0) {
            vel.x = 0;
        }
        if(collidedUp && vel.y > 0) {
            vel.y = 0;
        }
        if(collidedDown && vel.y < 0) {
            vel.y = 0;
        }
    }
    
    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        drawRect(playerBounds.center+transform.position, playerBounds.size);
        
        Vector2 min = playerBounds.center+transform.position-playerBounds.size/2;
        Vector2 max = playerBounds.center+transform.position+playerBounds.size/2;
        drawLines(Vector2.up,       new Vector2(min.x, max.y), max);
        drawLines(Vector2.down,     min, new Vector2(max.x, min.y));
        drawLines(Vector2.right,    new Vector2(max.x, min.y), max);
        drawLines(Vector2.left,     min, new Vector2(min.x, max.y));
    }
    
    void drawRect(Vector3 center, Vector3 size){
        Vector2 c1 = center - size/2;
        Vector2 c2 = c1 + new Vector2(size.x, 0);
        Vector2 c3 = c1 + new Vector2(0, size.y);
        Vector2 c4 = c1 + new Vector2(size.x, size.y);
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c1, c3);
        Gizmos.DrawLine(c2, c4);
        Gizmos.DrawLine(c3, c4);
    }
    
    void drawLines(Vector2 dir, Vector2 start, Vector2 end){
        for(int i = 0;i < rayCount;i++){
            Vector2 startPos = Vector2.Lerp(start, end, (float)i/(rayCount-1));
            Gizmos.DrawLine(startPos, startPos+dir*collidedCheckRange);
        }
    }
    
}
