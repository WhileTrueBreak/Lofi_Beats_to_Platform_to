using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    
    public float wallJumpBufferTime;
    public float jumpBufferTime;
    public float coyoteBufferTime;
    
    public float gravity;
    private float currentGravity;
    
    private bool allowCoyote = false;
    
    private Vector3 vel;
    
    private float lastGrounded;
    private bool isGrounded = false;
    private bool collidedDown = false;
    private bool collidedRight = false;
    private bool collidedLeft = false;
    private bool collidedUp = false;
    
    [SerializeField] private Bounds playerBounds;
    
    // Start is called before the first frame update
    void Start() {
        vel = new Vector2(0,0);
        currentGravity = gravity;
    }
    
    private bool _active;
    void Awake() => Invoke(nameof(Activate), 0.5f);
    void Activate() =>  _active = true;

    // Update is called once per frame
    void Update() {
        if(!_active) return;
        getInputs();
        checkCollisions();
        move();
    }
    
    private bool inputLeft = false;
    private bool inputRight = false;
    private bool inputJump = false;
    private bool inputDash = false;
    private float lastJumpInput = -1f;
    private float lastDashInput = -1f;
    
    void getInputs(){
        // get key input
        inputLeft = Input.GetKey("left");
        inputRight = Input.GetKey("right");
        inputJump = Input.GetKeyDown("up");
        inputDash = Input.GetKeyDown("space");
        
        if(inputJump) lastJumpInput = Time.time;
        if(inputDash) lastDashInput = Time.time;
    }
    
    [SerializeField] private LayerMask ground;
    [SerializeField] private float collidedCheckRange;
    
    void checkCollisions(){
        Vector2 min = playerBounds.center+transform.position-playerBounds.size/2;
        Vector2 max = playerBounds.center+transform.position+playerBounds.size/2;
        collidedUp = castRays(Vector2.up,       new Vector2(min.x, max.y), max);
        collidedDown = castRays(Vector2.down,   min, new Vector2(max.x, min.y));
        collidedRight = castRays(Vector2.right, new Vector2(max.x, min.y), max);
        collidedLeft = castRays(Vector2.left,   min, new Vector2(min.x, max.y));
        
        // if(collidedUp)
        //     Debug.Log("up: " + collidedUp);
        // if(collidedDown)
        //     Debug.Log("down: " + collidedDown);
        // if(collidedLeft)
        //     Debug.Log("left: " + collidedLeft);
        // if(collidedRight)
        //     Debug.Log("right: " + collidedRight);
        
        // collidedDown = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.down*collidedCheckRange, playerBounds.size, 0f, ground);
        // collidedUp = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.up*collidedCheckRange, playerBounds.size, 0f, ground);
        // collidedLeft = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.left*collidedCheckRange, playerBounds.size, 0f, ground);
        // collidedRight = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.right*collidedCheckRange, playerBounds.size, 0f, ground);
        
        // check if just left ground
        if(isGrounded && !collidedDown) lastGrounded = Time.time;
        if(collidedDown && !isGrounded) allowCoyote = true;
        
        isGrounded = collidedDown;
        
    }
    
    [SerializeField] private int rayCount;
    
    bool castRays(Vector2 dir, Vector2 start, Vector2 end){
        for(int i = 0;i < rayCount;i++){
            Vector2 startPos = Vector2.Lerp(start, end, (float)i/(rayCount-1));
            bool hit = Physics2D.Raycast(startPos, dir, collidedCheckRange, ground);
            if(hit) return true;
        }
        return false;
    }
    
    void move(){
        
        calcWalk();
        calcAerial();
        calcGrab();
        calcJump();
        calcWallJump();
        calcGravity();
        cancelVel();
        calcLatestCollision();
    }
    
    public float maxHorizontalSpeed;
    public float groundedHorizontalAccelTime;
    public float groundedHorizontalDecelTime;
    public float aerialHorizontalAccelTime;
    public float aerialHorizontalDecelTime;
    
    void calcWalk(){
        if(!isGrounded) return;
        // get the current horizontal vel
        var horizontalVel = vel.x;
        // if both down or up do nothing
        var acc = 0f;
        if (!inputLeft ^ inputRight) {
            // vel.x = 0;
            acc = Mathf.Min(maxHorizontalSpeed/(isGrounded?groundedHorizontalDecelTime:aerialHorizontalDecelTime), Mathf.Abs(horizontalVel)/Time.deltaTime) * -Mathf.Sign(horizontalVel);
        }
        // calc horizontal acc
        else if(inputLeft) acc = -maxHorizontalSpeed/(groundedHorizontalAccelTime);
        else if(inputRight) acc = maxHorizontalSpeed/(groundedHorizontalAccelTime);
        // added to vel
        vel.x += acc*Time.deltaTime;
        vel.x = Mathf.Clamp(vel.x, -maxHorizontalSpeed, maxHorizontalSpeed);
    }
    
    private bool disableAerial = false;
    
    void calcAerial(){
        if(disableAerial) return;
        if(isGrounded) return;
        // get the current horizontal vel
        var horizontalVel = vel.x;
        // if both down or up do nothing
        var acc = 0f;
        if (!inputLeft ^ inputRight) {
            // vel.x = 0;
            acc = Mathf.Min(maxHorizontalSpeed/aerialHorizontalDecelTime, Mathf.Abs(horizontalVel)/Time.deltaTime) * -Mathf.Sign(horizontalVel);
        }
        // calc horizontal acc
        else if(inputLeft) acc = -maxHorizontalSpeed/(aerialHorizontalAccelTime);
        else if(inputRight) acc = maxHorizontalSpeed/(aerialHorizontalAccelTime);
        // added to vel
        vel.x += acc*Time.deltaTime;
        vel.x = Mathf.Clamp(vel.x, -maxHorizontalSpeed, maxHorizontalSpeed);
    }
    
    private bool isGrabbing = false;
    private float lastGrabbed = -1;
    private int grabDir = 0;
    
    void calcGrab(){
        isGrabbing = false;
        grabDir = 0;
        if(isGrounded) return;
        if(!collidedLeft && !collidedRight) return;
        if(vel.y > 0) return;
        if(collidedLeft && inputLeft) grabDir = -1;
        if(collidedRight && inputRight) grabDir = 1;
        if(grabDir != 0){
            vel.y = 0;
            isGrabbing = true;
            lastGrabbed = Time.time;
        }
    }
    
    public float maxFallSpeed;
    
    void calcGravity(){
        if(isGrabbing) return;
        if(!collidedDown){
            vel.y += currentGravity*Time.deltaTime;
            vel.y = Mathf.Max(vel.y, -maxFallSpeed);
        }
    }
    
    public float jumpForce = 20;
    
    private bool isJumping = false;
    
    void calcJump(){
        if(isGrounded) isJumping = false;
        calcJumpHangtime();
        if(Time.time-lastJumpInput > jumpBufferTime && !inputJump) return;
        if(!(isGrounded || (Time.time-lastGrounded < coyoteBufferTime)&&allowCoyote)) return;
        lastJumpInput = -1;
        allowCoyote = false;
        isJumping = true;
        vel.y = jumpForce;
    }
    
    public float wallJumpForce = 20;
    public float wallJumpLockDirTime = 0.2f;
    private float lastWallJump = -1;
    
    void calcWallJump(){
        calcJumpHangtime();
        calcWallJumpTime();
        if(!isGrabbing) return;
        if(!inputJump) return;
        if(Time.time-lastGrabbed > wallJumpBufferTime) return;
        
        lastGrabbed = -1;
        isJumping = true;
        
        Vector3 jumpVector = Vector3.up + Vector3.left;
        jumpVector.Normalize();
        jumpVector.x *= grabDir;
        Debug.Log(jumpVector * wallJumpForce);
        vel += jumpVector * wallJumpForce;
        lastWallJump = Time.time;
    }
    
    void calcWallJumpTime(){
        disableAerial = false;
        if(Time.time - lastWallJump < wallJumpLockDirTime){
            disableAerial = true;
        }
    }
    
    public float apexRange = 10;
    public float apexGravity = -10;
    
    void calcJumpHangtime(){
        if(!isJumping) {
            currentGravity = gravity;
            return;
        }
        float dist = Mathf.InverseLerp(apexRange, 0, Mathf.Abs(vel.y));
        currentGravity = Mathf.Lerp(gravity, apexGravity, dist);
    }
    
    void cancelVel(){
        if(collidedRight && vel.x > 0) vel.x = 0;
        if(collidedLeft && vel.x < 0) vel.x = 0;
        if(collidedUp && vel.y > 0) vel.y = 0;
        if(collidedDown && vel.y < 0) vel.y = 0;
    }
    
    private int maxIterationSteps = 10;
    
    void calcLatestCollision(){
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
