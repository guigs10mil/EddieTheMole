using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndHole : MonoBehaviour
{
    public string sceneName = "Hub";

    private AudioManager audioManager;

    private void Start() {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void EnterHole() {
        StartCoroutine("EndLevelSequence");
    }

    IEnumerator EndLevelSequence() {
        audioManager.StopGroup("Music");
        PlayerPrefs.SetInt("LevelProgress", 0);
        GetComponentInChildren<Animator>().Play("endSequence");
        yield return new WaitForSecondsRealtime(3f);
        FindObjectOfType<GameManager>().changeScene(sceneName);
    }
}
