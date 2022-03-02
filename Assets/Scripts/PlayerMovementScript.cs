using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovementScript : MonoBehaviour {

    #region variables

    Rigidbody2D rb;

    Vector2 velocity;
    bool grounded;

    Vector2 floorUp = Vector2.up;
    Vector2 floorRight = Vector2.right;

    // horizontal movement
    public float groundSpeed;
    public float airSpeed;
    public float accelFriction;
    public float decelFriction;
    public float maxFloorAngle;

    //vertical movement
    public Vector2 jumpVector;
    public float jumpBufferTime; float jumpBufferTimer;
    public float gravityScale;
    public float coyoteTime; float coyoteTimer;
    public float slideSpeed;
    public float maxFallSpeed;

    public int groundMask;

    #endregion

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        jumpBufferTimer = jumpBufferTime;
    }

    #region controller input

    PlayerControls controls;

    Vector2 joystick;
    int joystickPressed;

    float movement;
    bool controllerJumpInput;
    bool jumpInput;
    bool jumpInputLastFrame;

    bool debugReset;
    bool resetLastFrame;

    private void Awake() {
        controls = new PlayerControls();

        controls.Player.Movement.performed += ctx =>
        {
            joystick = ctx.ReadValue<Vector2>();
            joystickPressed = 1;
        };
        controls.Player.Movement.canceled += ctx => joystickPressed = 0;

        controls.Player.Jump.performed += ctx => controllerJumpInput = true;
        controls.Player.Jump.canceled += ctx => controllerJumpInput = false;

        controls.Player.DebugReset.performed += ctx => debugReset = true;
        controls.Player.DebugReset.canceled += ctx => debugReset = false;
    }

    private void OnEnable() {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    #endregion

    private void Update() {


        #region input

        movement = joystickPressed * joystick.x;

        jumpInput = false;
        if (controllerJumpInput) {
            if (!jumpInputLastFrame) jumpInput = true;
            jumpInputLastFrame = true;
        }
        else jumpInputLastFrame = false;


        #endregion


        #region horizontal movement

        float currentFriction = movement == 0 ? decelFriction : accelFriction;
        float groundVel = groundSpeed * (1 / currentFriction - 1);
        float maxAirVel = jumpVector.x + groundSpeed;

        velocity.x = (velocity.x + movement
                   * (grounded ? groundVel : airSpeed))
                   * (grounded ? currentFriction : 1);

        velocity.x = Mathf.Clamp(velocity.x, -maxAirVel, maxAirVel);

        #endregion


        #region vertical movement

        jumpBufferTimer = jumpInput ? -0 : (jumpBufferTimer + Time.deltaTime);
        coyoteTimer     = grounded  ? -0 : (coyoteTimer + Time.deltaTime);
        bool jumping    = jumpBufferTimer < jumpBufferTime && (grounded || coyoteTimer < coyoteTime);

        if (grounded && velocity.y <= 0) velocity.y = -5;
        else velocity.y += gravityScale * Physics.gravity.y * Time.deltaTime;

        GetComponent<SpriteRenderer>().color = grounded && velocity.y > 0 ? Color.red : Color.white;

        velocity.y = Mathf.Max(velocity.y, maxFallSpeed);

        if (jumping) {

            velocity.y = Mathf.Sqrt(2 * gravityScale * Mathf.Abs(Physics.gravity.y) * jumpVector.y);
            velocity.x += jumpVector.x * Math.Sign(movement) * Mathf.Abs(velocity.x) / groundSpeed; // using a different sign function because mathf is weird

            jumpBufferTimer = coyoteTimer = Mathf.Infinity;

            floorUp = Vector2.up;
            floorRight = Vector2.right;
        }

        #endregion


        #region applying velocity

        rb.velocity = floorRight * velocity.x + floorUp * velocity.y;

        #endregion


        #region debug

        if (debugReset) {
            if (!resetLastFrame) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

                /* transform.position = Vector3.zero;
                 * rb.velocity = Vector2.zero;
                 * FindObjectOfType<Camera>().transform.position = Vector2.zero;*/
            }
            resetLastFrame = true;
        }
        else resetLastFrame = false;

        // GetComponent<SpriteRenderer>().color = grounded ? Color.green : Color.white;

        // Debug.DrawLine(transform.position, (Vector2)transform.position + floorUp * 2, Color.red);
        // Debug.DrawLine(transform.position, (Vector2)transform.position + floorRight * 2, Color.blue);

        #endregion
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.layer != groundMask) return;

        floorUp = collision.contacts[0].normal.normalized;
        floorRight = Quaternion.AngleAxis(-90, Vector3.forward) * floorUp;

        float floorAngle = 90 - Mathf.Rad2Deg * Mathf.Abs(Mathf.Atan2(floorUp.y, Mathf.Abs(floorUp.x)));
        grounded = floorAngle <= maxFloorAngle;

        if (!grounded) {
            floorUp = Vector2.up;
            floorRight = Vector2.right;

            // velocity.y -= slideSpeed * Mathf.Abs(Physics.gravity.y);
            velocity.x = 0;
        }
        else if (velocity.y <= 0)velocity.y = -5;
    }

    private void OnCollisionExit2D(Collision2D collision) {
        floorUp = Vector2.up;
        floorRight = Vector2.right;
        grounded = false;
    }

    #region ledge grabbing

    public void ledgeGrabTrigger(Collider2D collider, int dir) {
        if (LayerMask.LayerToName(collider.gameObject.layer) == "ground") {
            if (!grounded && Math.Sign(movement) == dir)
                StartCoroutine(ledgePullUp(dir));
        }
    }

    IEnumerator ledgePullUp(int dir) {
        yield return null;
    }

    #endregion
}