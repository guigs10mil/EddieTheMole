using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthDisplay : MonoBehaviour
{
    private Slider healthSlider;

    private void Start() {
        healthSlider = GetComponent<Slider>();
        if (!PlayerPrefs.HasKey("Health") || PlayerPrefs.GetInt("Health") == 0)
            PlayerPrefs.SetInt("Health", 3);
        healthSlider.value = PlayerPrefs.GetInt("Health");
    }

    public void UpdateHealthDisplay() {
        healthSlider.value = PlayerPrefs.GetInt("Health");
        transform.DOPunchScale(Vector3.one * 0.1f, 0.16f, 14);
    }
}
