using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector3 startPosition;
    private GameObject attackCollider;
    private PlayerMovement pm;
    private AudioManager audioManager;

    private HealthDisplay healthDisplay;

    private int maxHealth = 3;
    private int health = 3;
    private bool imune = false;

    private void Start() {
        startPosition = transform.position;
        attackCollider = transform.GetChild(1).gameObject;
        pm = GetComponent<PlayerMovement>();
        audioManager = FindObjectOfType<AudioManager>();
        healthDisplay = FindObjectOfType<HealthDisplay>();
        if (!PlayerPrefs.HasKey("Health") || PlayerPrefs.GetInt("Health") == 0)
            PlayerPrefs.SetInt("Health", maxHealth);
        health = PlayerPrefs.GetInt("Health");
        if (PlayerPrefs.GetInt("LevelProgress") == 1) {
            GameObject bonus = GameObject.Find("BonusStartPosition");
            if (bonus)
                transform.position = bonus.transform.position;
        }
    }

    public void Die() {
        Time.timeScale = 0f;
        audioManager.StopGroup("Music");
        StartCoroutine("Death");
    }

    public void Attack() {
        StartCoroutine("AttackDuration");
    }

    public void TakeDamage(GameObject source) {
        if (!pm.attacking && !imune) {
            if (!pm.drilling) {
                health--;
                UpdateHealthDisplay();
                if (health <= 0)
                    Die();
                else {
                    audioManager.Play("Hurt");
                    pm.DamageKnokback(source.transform.position);
                    StartCoroutine("ImunityTimer");
                }
            }
            else if (!pm.underground) {
                pm.BounceOffEnemy();
                source.GetComponent<Enemy>().BouncedOn();
            }
        }
    }

    public void TakeDamageFromHazard() {
        health--;
        UpdateHealthDisplay();
        if (health <= 0)
            Die();
        else {
            audioManager.Play("Hurt");
            pm.DamageKnokbackFromHazard();
            StartCoroutine("ImunityTimer");
        }
    }

    public void RegenerateHealth(int amount) {
        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }
        UpdateHealthDisplay();
    }

    void UpdateHealthDisplay() {
        if (healthDisplay != null)
            PlayerPrefs.SetInt("Health", health);
            healthDisplay.UpdateHealthDisplay();
    }

    IEnumerator AttackDuration()
    {
		attackCollider.SetActive(true);
        pm.attacking = true;
        yield return new WaitForSeconds(0.1f);
        attackCollider.SetActive(false);
        pm.attacking = false;
    }

    IEnumerator ImunityTimer()
    {
		imune = true;
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.1f);
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        imune = false;
    }

    IEnumerator Death() {
        PlayerPrefs.SetInt("LevelProgress", 0);
        yield return new WaitForSecondsRealtime(1f);
        PlayerPrefs.DeleteKey("Health");
        FindObjectOfType<GameManager>().changeScene("Hub");
    }
}
