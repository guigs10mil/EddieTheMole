using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col) {
        print(col.tag);
        if (col.tag == "Player") {
            col.GetComponent<Player>().Die();
        }
    }
}
