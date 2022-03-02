using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour {
    #region variables

    Rigidbody2D rb;
    BoxCollider2D col;

    public LayerMask groundMask;
    public float groundCheckDist;

    float movement;
    bool jumpPressed;

    public float groundSpeed;
    public float airSpeed;
    public float accelFriction;
    public float decelFriction;

    public Vector2 jumpVector;
    public float jumpBufferTime;
    float jumpBufferTimer;
    public float coyoteTime;
    float coyoteTimer;

    Vector2 velocity;
    bool grounded;

    float ogGravityScale;

    #endregion

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        ogGravityScale = rb.gravityScale;
    }

    private void Update() {

        #region state of the player

        velocity = rb.velocity;
        grounded = Physics2D.OverlapBox(transform.position + Vector3.down * col.bounds.extents.y, new Vector2(col.bounds.size.x, groundCheckDist), 0, groundMask);

        #endregion


        #region input

        movement = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");

        #endregion


        #region horizontal movement

        float currentFriction = movement == 0 ? decelFriction : accelFriction;
        float groundVel = groundSpeed * (1 / currentFriction - 1);
        float maxAirVel = jumpVector.x + groundSpeed;

        velocity.x = (velocity.x + movement * (grounded ? groundVel : airSpeed)) * (grounded ? currentFriction : 1);

        velocity.x = Mathf.Clamp(velocity.x, -maxAirVel, maxAirVel);

        print(velocity.x + " : " + maxAirVel);

        #endregion


        #region vertical movement

        jumpBufferTimer += Time.deltaTime;
        coyoteTimer += Time.deltaTime;

        if (jumpPressed) jumpBufferTimer = -0;
        if (grounded) coyoteTimer = -0;

        if (jumpBufferTimer < jumpBufferTime && (grounded || coyoteTimer < coyoteTime)) {

            velocity.y = Mathf.Sqrt(2 * rb.gravityScale * Mathf.Abs(Physics.gravity.y) * jumpVector.y);
            velocity.x += jumpVector.x * Math.Sign(movement) * Mathf.Abs(velocity.x) / groundSpeed; // using a different sign function because mathf is weird

            jumpBufferTimer = coyoteTimer = Mathf.Infinity;
        }

        #endregion


        rb.velocity = velocity;


        #region debug

        if (Input.GetKeyDown(KeyCode.R)) {
            transform.position = Vector3.zero;
            rb.velocity = Vector2.zero;
            FindObjectOfType<Camera>().transform.position = Vector2.zero;
        }

        #endregion
    }

    public void ledgeGrabTrigger(Collider2D collider, int dir) {
        if (LayerMask.LayerToName(collider.gameObject.layer) == "ground") {
            if (!grounded && Math.Sign(movement) == dir)
                StartCoroutine(ledgePullUp(dir));
        }
    }

    IEnumerator ledgePullUp(int dir) {
        yield return null;
    }
}