using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator doorAnimator; // Kap�y� kontrol etmek i�in Animator bile�eni
    public float delayBeforeOpening = 10f; // Kap�n�n a��lma gecikme s�resi (10 saniye)

    private void Start()
    {
        // Belirtilen s�re sonunda kap�y� a�
        Invoke("OpenDoor", delayBeforeOpening);
    }

    private void OpenDoor()
    {
        if (doorAnimator != null)
        {
            // Kap�y� a�mak i�in animasyonu tetikle
            doorAnimator.SetTrigger("OpenDoor");
        }
        else
        {
            Debug.LogError("Door Animator is not assigned.");
        }
    }
}
