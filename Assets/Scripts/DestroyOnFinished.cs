using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnFinished : MonoBehaviour
{
    ParticleSystem ps;

    private void Start() {
        ps = GetComponent<ParticleSystem>();

        Destroy(gameObject, ps.main.duration - .1f);
    }
}
