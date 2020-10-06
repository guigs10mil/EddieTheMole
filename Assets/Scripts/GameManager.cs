using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    GameObject transition;

    private void Start() {
        Time.timeScale = 1f;
        transition = Camera.main.transform.GetChild(0).gameObject;
        transition.SetActive(true);
        StartCoroutine(ChangeSceneIn());

        if (!PlayerPrefs.HasKey("LevelProgress"))
            PlayerPrefs.SetInt("LevelProgress", 0);

        if (SceneManager.GetActiveScene().name == "LVL01")
            GetComponent<AudioManager>().Play("Overground");

        if (SceneManager.GetActiveScene().name == "LVL02-1")
            GetComponent<AudioManager>().Play("Underground");
    }

    // private void Update() {
    //     var keyboard = Keyboard.current;
	// 	var gamepad = Gamepad.current;
    //     if (gamepad == null)
	// 		print("No gamepad connected.");
    // }

    public void changeScene(string sceneName) {
        StartCoroutine(ChangeSceneOut(sceneName));
    }

    IEnumerator ChangeSceneOut(string sceneName) {
        transition.transform.GetChild(0).GetComponent<Animator>().Play("SceneOut");
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ChangeSceneIn() {
        transition.transform.GetChild(0).GetComponent<Animator>().Play("SceneIn");
        yield return new WaitForSecondsRealtime(1f);
        // transition.SetActive(false);
    }
}
