using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector3 startPosition;
    private GameObject attackCollider;
    PlayerMovement pm;

    // private bool attacking = false;

    private void Start() {
        startPosition = transform.position;
        attackCollider = transform.GetChild(1).gameObject;
        pm = GetComponent<PlayerMovement>();
    }

    public void Die() {
        transform.position = startPosition;
    }

    public void Attack() {
        StartCoroutine("AttackDuration");
    }

    public void TakeDamage() {
        if (!pm.attacking && !pm.drilling) {
            Die();
        }
    }

    IEnumerator AttackDuration()
    {
		attackCollider.SetActive(true);
        pm.attacking = true;
        yield return new WaitForSeconds(0.1f);
        attackCollider.SetActive(false);
        pm.attacking = false;
    }
}
