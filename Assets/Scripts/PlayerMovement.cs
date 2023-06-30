using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    
    public float wallJumpBufferTime = 0.5f;
    public float jumpBufferTime = 0.5f;
    public float coyoteBufferTime = 0.5f;
    
    private bool allowCoyote = false;
    
    private Vector3 vel;
    
    // public float jumpForce = 10;
    
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
    }
    
    private bool _active;
    void Awake() => Invoke(nameof(Activate), 0.5f);
    void Activate() =>  _active = true;

    // Update is called once per frame
    void Update() {
        if(!_active) return;
        checkCollisions();
        move();
    }
    
    [SerializeField] private LayerMask ground;
    private float collidedCheckRange = 0.01f;
    
    void checkCollisions(){
        collidedDown = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.down*collidedCheckRange, playerBounds.size, 0f, ground);
        collidedUp = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.up*collidedCheckRange, playerBounds.size, 0f, ground);
        collidedLeft = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.left*collidedCheckRange, playerBounds.size, 0f, ground);
        collidedRight = Physics2D.OverlapBox(playerBounds.center+transform.position+Vector3.right*collidedCheckRange, playerBounds.size, 0f, ground);
        
        // check if just left ground
        if(isGrounded && !collidedDown) lastGrounded = Time.time;
        if(collidedDown && !isGrounded) allowCoyote = true;
        
        isGrounded = collidedDown;
        
    }
    
    void move(){
        calcHorizontal();
        calcGravity();
        calcJump();
        calcLatestCollision();
    }
    
    private bool left = false;
    private bool right = false;
    private bool up = false;
    
    public float maxHorizontalSpeed = 4f;
    public float groundedHorizontalAccelTime = 2f;
    public float groundedHorizontalDecelTime = 3f;
    public float aerialHorizontalAccelTime = 2f;
    public float aerialHorizontalDecelTime = 3f;
    
    void calcHorizontal(){
        // get the current horizontal vel
        var horizontalVel = vel.x;
        // get key input
        var left = Input.GetKey("left");
        var right = Input.GetKey("right");
        // if both down or up do nothing
        var acc = 0f;
        if (!left ^ right) {
            // vel.x = 0;
            acc = Mathf.Min(maxHorizontalSpeed/(isGrounded?groundedHorizontalDecelTime:aerialHorizontalDecelTime), Mathf.Abs(horizontalVel)/Time.deltaTime) * -Mathf.Sign(horizontalVel);
        }
        // calc horizontal acc
        else if(left) acc = -maxHorizontalSpeed/(isGrounded?groundedHorizontalAccelTime:aerialHorizontalAccelTime);
        else if(right) acc = maxHorizontalSpeed/(isGrounded?groundedHorizontalAccelTime:aerialHorizontalAccelTime);
        // added to vel
        vel.x += acc*Time.deltaTime;
        vel.x = Mathf.Clamp(vel.x, -maxHorizontalSpeed, maxHorizontalSpeed);
        
        // stop if colliding
        if((collidedLeft && vel.x < 0)||(collidedRight && vel.x > 0)) vel.x = 0;
    }
    
    public float maxFallSpeed = 10f;
    public float gravity = -20f;
    
    void calcGravity(){
        if(!collidedDown){
            vel.y += gravity*Time.deltaTime;
            vel.y = Mathf.Max(vel.y, -maxFallSpeed);
        }
        if(collidedDown && vel.y < 0) vel.y = 0;
    }
    
    
    void calcJump(){
        if(!Input.GetKey("up")) return;
        if(!(isGrounded || (Time.time-lastGrounded < coyoteBufferTime)&&allowCoyote)) return;
        allowCoyote = false;
        vel.y = 10;
        
        if(collidedUp && vel.y > 0) vel.y = 0;
    }
    
    private int maxIterationSteps = 10;
    
    void calcLatestCollision(){
        if(vel.magnitude == 0) return;
        
        var moveStep = vel*Time.deltaTime;
        var pos = playerBounds.center+transform.position;
        var endPos = pos + moveStep;
        var hasCollision = Physics2D.OverlapBox(endPos, playerBounds.size, 0, ground);
        if(hasCollision == null){
            transform.position += moveStep;
            return;
        }
        var lastValid = 0f;
        var currentPercentage = 0.5f;
        var nextStep = 0.25f;
        for(var i = 0;i < maxIterationSteps;i++){
            var posToTest = Vector3.Lerp(pos, endPos, currentPercentage);
            var collided = Physics2D.OverlapBox(endPos, playerBounds.size, 0, ground);
            if(collided) currentPercentage -= nextStep;
            else{
                lastValid = currentPercentage;
                currentPercentage += nextStep;
            }
            nextStep /= 2;
        }
        moveStep.Normalize();
        transform.position += moveStep*currentPercentage;
    }
    
    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        drawRect(playerBounds.center+transform.position, playerBounds.size);
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
    
    
}
