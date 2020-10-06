using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Enemy") {
            col.GetComponent<Enemy>().Die(transform.position);
        }
        if (col.tag == "HealthCollectable") {
            col.GetComponent<HealthCollectable>().Collect();
        }
    }
}
