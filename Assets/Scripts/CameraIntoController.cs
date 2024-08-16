using UnityEngine;
using Photon.Pun;

public class CameraIntroController : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject gameUI;
    public float animationDuration = 5f;
    public CameraFollow cameraFollow;
    private PlayerMovement[] playerMovements; // PlayerMovement referanslar�n� saklayacak dizi
    public GhostController ghostController; // GhostController referans�

    private float timer;
    private bool hasAnimationCompleted = false; // Animasyonun tamamlan�p tamamlanmad���n� kontrol etmek i�in

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

        // Ghost'un hareketini ba�lang��ta durdur
        if (ghostController != null)
        {
            ghostController.enabled = false;
        }
    }

    void Update()
    {
        if (!hasAnimationCompleted)
        {
            if (cameraAnimator.GetCurrentAnimatorStateInfo(0).IsName("TrapPGStartAnim"))
            {
                timer += Time.deltaTime;
                if (timer >= animationDuration)
                {
                    OnAnimationComplete();
                    hasAnimationCompleted = true; // Animasyon tamamland���nda bunu i�aretle
                }
            }
        }
    }

    public void OnAnimationComplete()
    {
        if (gameUI != null)
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

        // Ghost hareketini ba�lat
        if (ghostController != null)
        {
            ghostController.enabled = true;
        }

        StartGame();
    }

    void StartGame()
    {
        // Oyun ba�lang�� i�lemlerini buraya ekleyin
    }
}
