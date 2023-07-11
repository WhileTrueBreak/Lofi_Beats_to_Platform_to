using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementV2 : MonoBehaviour {
    #region variables
    public Vector3 vel;
    public Vector3 facing;
    
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
    
    [SerializeField] BeatManager _beatManager;
    private bool _isOnBeat;

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
        
        if(inputJump){
            lastJumpInput = Time.time;
            checkTiming();
        }
        if(inputDash) {
            lastDashInput = Time.time;
            checkTiming();
        }
        if(inputLeft || inputRight) lastMoveInput = Time.time;
        else lastStopInput = Time.time;
    }

    void checkTiming(){
        _isOnBeat = _beatManager.IsOnBeat();
    }
    
    #region collision
    [SerializeField] private Bounds playerBounds;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float collidedCheckRange;
    [SerializeField] private int rayCount;
    #endregion
    
    private float lastGrounded;
    private float lastInAir;
    private bool isGrounded = false;
    private bool collidedDown = false;
    private bool collidedRight = false;
    private bool collidedLeft = false;
    private bool collidedUp = false;
    
    public float collisionBumpBuffer;
    
    void checkCollisions(){
        Vector2 offset = new Vector2(0,0);
        getCollideDirs(offset);
        // test vertical offset
        // get hoz direction
        if(vel.x > 0 && collidedRight){
            getCollideDirs(new Vector2(0, collisionBumpBuffer));
            if(!collidedRight)offset.y =  collisionBumpBuffer;
            getCollideDirs(new Vector2(0,-collisionBumpBuffer));
            if(!collidedRight)offset.y = -collisionBumpBuffer;
        }else if(vel.x < 0 && collidedLeft){
            getCollideDirs(new Vector2(0, collisionBumpBuffer));
            if(!collidedLeft)offset.y  =  collisionBumpBuffer;
            getCollideDirs(new Vector2(0,-collisionBumpBuffer));
            if(!collidedLeft)offset.y  = -collisionBumpBuffer;
        }
        // get up direction
        if(vel.y > 0 && collidedUp){
            getCollideDirs(new Vector2( collisionBumpBuffer, 0));
            if(!collidedUp) offset.x =  collisionBumpBuffer;
            getCollideDirs(new Vector2(-collisionBumpBuffer, 0));
            if(!collidedUp) offset.x = -collisionBumpBuffer;
        }
        
        //check if new offset position collides
        Vector3 playerPos = playerBounds.center+transform.position;
        Vector3 playerOffset = offset;
        var hasCollision = Physics2D.OverlapBox(playerPos+playerOffset, playerBounds.size, 0, ground);
        
        //if no collision move player
        if(!hasCollision)
        transform.position += playerOffset;
        
        //get final collisions
        getCollideDirs(new Vector2(0,0));
        
        isGrounded = collidedDown;
        
        if(isGrounded) lastGrounded = Time.time;
        else lastInAir = Time.time;
    }
    
    void getCollideDirs(Vector2 offset){
        Vector2 min = playerBounds.center+transform.position-playerBounds.size/2+offset;
        Vector2 max = playerBounds.center+transform.position+playerBounds.size/2+offset;
        collidedUp =    castRays(Vector2.up,    new Vector2(min.x, max.y), max);
        collidedDown =  castRays(Vector2.down,  min, new Vector2(max.x, min.y));
        collidedRight = castRays(Vector2.right, new Vector2(max.x, min.y), max);
        collidedLeft =  castRays(Vector2.left,  min, new Vector2(min.x, max.y));
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
    private bool allowWallGrab = false;
    private bool isJumping = false;
    private bool isJumpStart = false;
    private bool isDashing = false;
    private bool isSliding = false;
    private bool isWallGrabbing = false;
    
    // timers
    private float lastDash = -1;
    private float lastJump = -1;
    private float lastOnWall = -1;
    
    void setFlags(){
        resetFlags();
        checkJumpFlag();
        checkWallJumpFlag();
        checkDashFlag();
        checkWallGrab();
        checkWalkFlag();
        checkAerialFlag();
        checkGravityFlag();
    }
    
    void checkWallGrab(){
        if(collidedLeft  && facing.x < 0) allowWallGrab = true;
        if(collidedRight && facing.x > 0) allowWallGrab = true;
        if(inputDown) allowWallGrab = false;
        if(isDashing) allowWallGrab = false;
        if(!allowWallGrab) isWallGrabbing = false;
    }
    
    void resetFlags(){
        allowWalk = true;
        allowAerial = false;
        allowGravity = true;
        allowJump = false;
        allowWallJump = false;
        allowDash = false; 
        allowWallGrab = false;
    }
    
    void checkWalkFlag(){
        if(!isGrounded) allowWalk = false;
    }
    
    void checkAerialFlag(){
        if(!isGrounded) allowAerial = true;
        if(isWallGrabbing) allowAerial = false;
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
        
        // must be jumping to be at the start of a jump
        if(!isJumping) isJumpStart = false;
    }
    
    void checkWallJumpFlag(){
        // check if on wall and not on ground
        if(isWallGrabbing && !isGrounded) allowWallJump = true;
    }
    
    void checkDashFlag(){
        if(Time.time-lastInAir < dashBuffer) allowDash = true;
        
        // check if dash direction collides with anything
        if((collidedLeft && vel.x < 0) || (collidedRight && vel.x > 0) || (collidedDown && vel.y < 0) || (collidedUp && vel.y > 0)) {
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
    public float wallJumpInputBuffer;
    public float wallJumpAngle;
    public float wallJumpForce;
    #endregion
    // dash params
    #region dash
    public float dashBuffer;
    public float dashForce;
    public float dashDuration;
    public float dashEndSpeed;
    #endregion
    // wall grab params
    #region wall grab
    public float grabFallSpeed;
    #endregion
    
    void calcMovement(){
        calcWalk();
        calcAerialControl();
        calcJump();
        calcWallGrab();
        calcWallJump();
        calcJumpAirTime();
        calcDash();
        calcDashTime();
        calcGravity();
        cancelVel();
        calcNextPosition();
    }
    
    void calcWallGrab(){
        if(!allowWallGrab) return;
        // clamp max down speed to grab speed
        vel.y = Mathf.Clamp(vel.y, -grabFallSpeed, Mathf.Infinity);
        isWallGrabbing = true;
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
        
        else if(inputLeft)  {
            hozVel = Mathf.MoveTowards(hozVel, -maxWalkSpeed, maxWalkSpeed/walkAccelTime*Time.deltaTime);
            facing = Vector3.left;
        }
        else if(inputRight) {
            hozVel = Mathf.MoveTowards(hozVel,  maxWalkSpeed, maxWalkSpeed/walkAccelTime*Time.deltaTime);
            facing = Vector3.right;
        }
        
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
        
        else if(inputLeft) {
            hozVel = Mathf.MoveTowards(hozVel, -maxAerialSpeed, maxAerialSpeed/aerialAccelTime*Time.deltaTime);
            facing = Vector3.left;
        }
        else if(inputRight) {
            hozVel = Mathf.MoveTowards(hozVel,  maxAerialSpeed, maxAerialSpeed/aerialAccelTime*Time.deltaTime);
            facing = Vector3.right;
        }
        
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
        // check for last jump input
        if(Time.time-lastJumpInput > wallJumpInputBuffer) return;
        lastJumpInput = -1;
        isJumping = true;
        //calculate jump direction
        float rad = wallJumpAngle*Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0);
        dir.x *= -System.Math.Sign(facing.x);
        facing.x = -facing.x;
        
        vel = dir*wallJumpForce;
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
        
        facing = new Vector3(1*System.Math.Sign(dashVec.x),0,0);
        
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
