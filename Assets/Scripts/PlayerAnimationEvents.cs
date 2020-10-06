using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private AudioManager audioManager;

    private void Start() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void PlayFootstep() {
        audioManager.Play("FootStep" + Random.Range(1, 3).ToString());
    }
}
