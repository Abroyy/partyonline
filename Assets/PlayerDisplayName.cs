using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerDisplayName : MonoBehaviour
{
    public TMP_Text playerDisplayName;

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
    }

    [PunRPC]
    public void SetPlayerName(string name)
    {
        // Gelen ismi ekranda g�ster
        playerDisplayName.text = name;
    }
}
