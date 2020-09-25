using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public void Die(Vector3 sourcePosition) {
        GetComponent<EnemyMovement>().Die(sourcePosition);
    }
}
