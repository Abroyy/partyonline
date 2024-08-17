using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;

    private Camera mainCamera; // Ana kamera referans�

    private void Start()
    {
        string displayName = PlayerPrefs.GetString("DISPLAYNAME", "Guest");
        playerDisplayName.text = displayName;

        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            playerDisplayName.text = PhotonNetwork.NickName;
        }
        else
        {
            playerDisplayName.text = photonView.Owner.NickName;
        }

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
}
