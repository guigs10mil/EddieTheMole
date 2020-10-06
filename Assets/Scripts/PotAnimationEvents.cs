using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotAnimationEvents : MonoBehaviour
{
    public GameObject dirtParticles;
    private AudioManager audioManager;

    private void Start() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void SpawnDirtParticles() {
        Instantiate(dirtParticles, transform.position, transform.rotation);
    }

    public void OkSound() {
        audioManager.Play("Ok");
    }
}
