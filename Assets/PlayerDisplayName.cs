using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;

    private Camera mainCamera; // Ana kamera referans�

    private void Start()
    {
        // PlayerPrefs'ten DisplayName'i �ek
        string displayName = PlayerPrefs.GetString("DISPLAYNAME", "Unknown Player");

        // Photon RPC ile di�er oyunculara ad�n� g�nder
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, displayName);
        }

        // Ana kameray� bul
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // Metni kameraya do�ru d�nd�r
            transform.LookAt(mainCamera.transform);

            // Y eksenindeki d�n��� s�f�rla
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180f, 0);
        }
    }

    [PunRPC]
    public void SetPlayerName(string name)
    {
        // Gelen ismi ekranda g�ster
        playerDisplayName.text = name;
    }
}
