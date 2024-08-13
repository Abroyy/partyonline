using UnityEngine;
using Photon.Pun;

public class CameraIntroController : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;
    private PlayerMovement[] playerMovements; // PlayerMovement referanslar�n� saklayacak dizi

    private float timer;

    void Start()
    {
        timer = 0f;

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartGame");
        }

        // Joystick ve Jump butonlar�n� devre d��� b�rak ve Rigidbody'yi k�s�tla
        playerMovements = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement pm in playerMovements)
        {
            pm.DisableMovement(); // Hareketi tamamen devre d��� b�rak
        }
    }

    void Update()
    {
        if (cameraAnimator.GetCurrentAnimatorStateInfo(0).IsName("TrapPGStartAnim"))
        {
            timer += Time.deltaTime;
            if (timer >= animationDuration)
            {
                OnAnimationComplete();
            }
        }
    }

    public void OnAnimationComplete()
    {
        if (gameUI != null && !Hazard.IsPlayerDead())
        {
            gameUI.SetActive(true);
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartCameraFollow();
        }

        // Joystick ve Jump butonlar�n� tekrar aktif hale getir ve hareketi etkinle�tir
        foreach (PlayerMovement pm in playerMovements)
        {
            pm.EnableMovement(); // Hareketi tekrar etkinle�tir
        }

        StartGame();
    }

    void StartGame()
    {
        // Oyun ba�lang�� i�lemlerini buraya ekleyin
    }
}
