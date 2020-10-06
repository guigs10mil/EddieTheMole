using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHole : MonoBehaviour
{
    public string sceneName = "Hub";
    public int levelProgress = 0;

    public void EnterHole() {
        PlayerPrefs.SetInt("LevelProgress", levelProgress);
        GetComponentInChildren<Animator>().Play("entered");
        FindObjectOfType<GameManager>().changeScene(sceneName);
    }
}
