using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour {
    public Transform follow;

    [Tooltip("0 for instant following, approach 1 for slower speed")]
    public float followSpeed;

    public bool lockX;
    public bool lockY;
    public Vector2 lockPosition;

    private void FixedUpdate() {
        transform.position = Vector3.Lerp(follow.position, transform.position, followSpeed);
        
        transform.position = new Vector3(
            lockX ? lockPosition.x : transform.position.x,
            lockY ? lockPosition.y : transform.position.y,
            -10);
    }
}
