using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour {

    #region variables

    Rigidbody2D rb;
    BoxCollider2D col;

    Vector2 velocity;
    bool grounded;

    float movement;
    bool jumpPressed;

    Vector2 floorUp;
    Vector2 floorRight;

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

    public LayerMask groundMask;
    public float groundCheckDist;

    #endregion

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    private void Update() {

        #region state of the player

        grounded = Physics2D.OverlapBox(transform.position + Vector3.down * col.bounds.extents.y, new Vector2(col.bounds.size.x - 0.1f, groundCheckDist), 0, groundMask);
        float floorAngle = 90 - Mathf.Rad2Deg * Mathf.Abs(Mathf.Atan2(floorUp.y, Mathf.Abs(floorUp.x)));
        bool standing = floorAngle <= maxFloorAngle;

        #endregion


        #region input

        movement = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");

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

        jumpBufferTimer = jumpPressed ? -0 : (jumpBufferTimer + Time.deltaTime);
        coyoteTimer = grounded ? -0 : (coyoteTimer + Time.deltaTime);
        bool jumping = jumpBufferTimer < jumpBufferTime && (grounded || coyoteTimer < coyoteTime) && standing;

        if (grounded && velocity.y <= 0) velocity.y = Mathf.Min(velocity.y, rb.velocity.y);
        else velocity.y += gravityScale * Physics.gravity.y * Time.deltaTime;

        if (!standing) {
            velocity.y -= slideSpeed * Mathf.Abs(Physics.gravity.y);
            velocity.x = 0;
        }

        velocity.y = Mathf.Max(velocity.y, maxFallSpeed);

        if (jumping) {

            velocity.y = Mathf.Sqrt(2 * gravityScale * Mathf.Abs(Physics.gravity.y) * jumpVector.y);
            velocity.x += jumpVector.x * Math.Sign(movement) * Mathf.Abs(velocity.x) / groundSpeed; // using a different sign function because mathf is weird

            jumpBufferTimer = coyoteTimer = Mathf.Infinity;
        }

        if (jumping || velocity.y > 0 || !standing) {
            floorUp = Vector2.up;
            floorRight = Vector2.right;
        }

        #endregion


        #region applying velocity

        rb.velocity = floorRight * velocity.x + floorUp * velocity.y;

        #endregion


        #region debug

        if (Input.GetKeyDown(KeyCode.R)) {
            transform.position = Vector3.zero;
            rb.velocity = Vector2.zero;
            FindObjectOfType<Camera>().transform.position = Vector2.zero;
        }

        GetComponent<SpriteRenderer>().color = grounded ? Color.green : Color.white;

        Debug.DrawLine(transform.position, (Vector2)transform.position + floorUp * 2, Color.red);
        Debug.DrawLine(transform.position, (Vector2)transform.position + floorRight * 2, Color.blue);

        #endregion
    }

    private void OnCollisionStay2D(Collision2D collision) {
        floorUp = collision.contacts[0].normal.normalized;
        floorRight = Quaternion.AngleAxis(-90, Vector3.forward) * floorUp;
    }

    private void OnCollisionExit2D(Collision2D collision) {
        floorUp = Vector2.up;
        floorRight = Vector2.right;
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