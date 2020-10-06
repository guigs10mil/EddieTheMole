using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemy;

    public void Die(Vector3 sourcePosition) {
        GetComponent<EnemyMovement>().DieFromEmerging(sourcePosition);
    }

    public void BouncedOn() {
        if (enemy == "Slime")
            GetComponent<EnemyMovement>().Die();
        if (enemy == "Shelled")
            GetComponent<EnemyMovement>().Duck();
    }
}
