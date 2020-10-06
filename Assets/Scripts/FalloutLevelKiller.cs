using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalloutLevelKiller : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            col.GetComponent<Player>().Die();
        } else {
            Destroy(col.gameObject);
        }
    }
}
