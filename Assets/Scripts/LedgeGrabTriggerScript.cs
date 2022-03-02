using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabTriggerScript : MonoBehaviour {
    public int dir;

    private void OnTriggerEnter2D(Collider2D collision) {
        transform.parent.gameObject.GetComponent<PlayerMovementScript>().ledgeGrabTrigger(collision, dir);
    }
}
