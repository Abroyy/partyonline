using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // Kamera ile karakter aras�ndaki mesafe
    public float fixedXRotation = 36f; // Sabit X rotasyonu

    private Transform target;
    private bool followStarted = false; // Kamera takip sisteminin ba�lat�l�p ba�lat�lmad���n� kontrol etmek i�in

    private void LateUpdate()
    {
        if (followStarted && target != null)
        {
            // Kamera hedefin arkas�nda ve yukar�s�nda bir mesafede olacak �ekilde hesapla
            Vector3 desiredPosition = target.position + offset;

            // Kameray� hedefin yeni pozisyonuna yumu�ak bir �ekilde kayd�r
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Kameray� hedefe bakacak �ekilde ayarla
            // Ancak X rotasyonunu sabit tut
            Vector3 currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(fixedXRotation, currentRotation.y, currentRotation.z);
        }
    }

    public void StartCameraFollow()
    {
        // Sahnedeki t�m karakterleri kontrol et ve yerel oyuncuya ait olan� bul
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView photonView = playerObject.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                target = playerObject.transform;

                // Kamera'n�n X rotasyonunu sabit tut
                Vector3 currentRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(fixedXRotation, currentRotation.y, currentRotation.z);

                followStarted = true; // Takip sistemini ba�lat
                break;
            }
        }

        if (target == null)
        {
            Debug.LogError("Target not found. Ensure that the local player's character is tagged correctly as 'Player'.");
        }
    }
}
