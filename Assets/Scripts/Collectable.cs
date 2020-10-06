using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    private AudioManager audioManager;

    private void Start() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            audioManager.Play("Coin");
            Destroy(gameObject);
        }
    }
}
