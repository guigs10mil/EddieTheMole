using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectable : MonoBehaviour
{
    public void Collect() {
        FindObjectOfType<Player>().RegenerateHealth(2);
        Destroy(gameObject);
    }
}
