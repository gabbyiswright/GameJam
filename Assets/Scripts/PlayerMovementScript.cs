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

    public float groundSpeed;
    public float airSpeed;
    public float friction;

    public Vector2 jumpVector;
    public float jumpBufferTime;
    float jumpBufferTimer;
    public float coyoteTime;
    float coyoteTimer;

    Vector2 velocity;
    bool grounded;

    #endregion

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    private void Update() {

        #region state of the player

        velocity = rb.velocity;
        grounded = Physics2D.OverlapBox(transform.position + Vector3.down * col.bounds.extents.y, new Vector2(col.bounds.size.x, groundCheckDist), 0, groundMask);

        #endregion


        #region input

        float movement = Input.GetAxisRaw("Horizontal");
        bool jumpPressed = Input.GetButtonDown("Jump");

        #endregion


        #region horizontal movement

        float groundVel = groundSpeed * (1 / friction - 1);
        float maxAirVel = jumpVector.x + groundVel;

        velocity.x += movement * (grounded ? groundVel : airSpeed);
        if (grounded) velocity.x *= friction;

        print(movement == -Mathf.Sign(velocity.x));

        velocity.x = Mathf.Clamp(velocity.x, -maxAirVel, maxAirVel);

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
}