using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectHole : MonoBehaviour
{
    public GameObject levelSelectCanvas;

    public void EnterHole() {
        PlayerPrefs.SetInt("LevelProgress", 0);
        PlayerPrefs.DeleteKey("Health");
        GetComponentInChildren<Animator>().Play("entered");
        levelSelectCanvas.SetActive(true);
    }
}
