using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonLogic : MonoBehaviour
{
    private AudioManager audioManager;

    private void Start() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void WiggleButton() {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.16f, 14).SetUpdate(true);
        audioManager.Play("Submit");
    }

    public void IncreaseButton() {
        transform.DOScale(Vector3.one * 1.3f, 0.05f).SetUpdate(true);
        audioManager.Play("Tick");
    }

    public void DecreaseButton() {
        transform.DOScale(Vector3.one, 0.05f).SetUpdate(true);
    }
}
