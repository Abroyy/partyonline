using UnityEngine;
using System.Collections;

public class Hazard : MonoBehaviour
{
    private static bool isPlayerDead = false;
    public Transform deadPoint; // Dead point transform
    public GameObject gameUI;
    public Joystick joystick;
    public float deathAnimationCooldown = 3f; // Animasyonun tekrar �al��abilece�i s�re

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerDead) // Oyuncu ile temas ve animasyon tekrar �al��abilir durumda
        {
            StartCoroutine(HandlePlayerDeath(other.gameObject));
        }
    }

    private IEnumerator HandlePlayerDeath(GameObject player)
    {
        isPlayerDead = true;
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            gameUI.SetActive(false);
            animator.SetTrigger("Die");
            joystick.ResetHandlePosition();
        }

        // Oyuncunun alt�nda BoomEffect ad�nda bir ParticleSystem aray�n
        ParticleSystem boomEffect = player.GetComponentInChildren<ParticleSystem>();

        if (boomEffect != null)
        {
            boomEffect.Play(); // Partik�lleri ba�lat
        }
        else
        {
            Debug.LogError("BoomEffect ParticleSystem not found in the player!");
        }

        yield return new WaitForSeconds(2.16f); // Die animasyonunun s�resi
        player.SetActive(false);

        player.transform.position = deadPoint.position;
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        gameUI.SetActive(true);
        player.SetActive(true);
        boomEffect.Stop();

        yield return new WaitForSeconds(deathAnimationCooldown - 2.16f); // Kalan s�re i�in bekle

        isPlayerDead = false;
    }

    // Bu methodu static olarak tan�ml�yoruz
    public static bool IsPlayerDead()
    {
        return isPlayerDead;
    }
}
