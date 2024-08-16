using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviourPunCallbacks
{
    public GameObject gameOverPanel; // Game Over Paneli
    public Transform playerDataContainer; // Player verilerinin g�sterilece�i UI container

    // Her oyuncu i�in ayr� prefablar
    public GameObject playerDataPrefab1;
    public GameObject playerDataPrefab2;
    public GameObject playerDataPrefab3;
    public GameObject playerDataPrefab4;

    private Dictionary<string, PlayerData> allPlayerData = new Dictionary<string, PlayerData>();

    [System.Serializable]
    public class PlayerData
    {
        public string nickname;
        public int coins;
        public Sprite profileImage;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    [PunRPC]
    public void SyncPlayerData(string nickname, int coins, string profileImageName)
    {
        Sprite profileImage = Resources.Load<Sprite>("ProfileImages/" + profileImageName);

        PlayerData playerData = new PlayerData
        {
            nickname = nickname,
            coins = coins,
            profileImage = profileImage
        };

        if (!allPlayerData.ContainsKey(nickname))
        {
            allPlayerData.Add(nickname, playerData);
        }

        // Update UI
        UpdateGameOverPanel();
    }

    public void OnGameOver()
    {
        int earnedCoins = CoinManager.Instance.GetCurrentCoins();
        string playerNickname = PlayerPrefs.GetString("DISPLAYNAME"); // PlayFab'den �ekilen nickname
        string profileImageName = "defaultProfileImage"; // Kullan�c�n�n profil g�r�nt�s�n�n ad�n� buraya ekleyin

        photonView.RPC("SyncPlayerData", RpcTarget.All, playerNickname, earnedCoins, profileImageName);
    }

    void UpdateGameOverPanel()
    {
        // Eski UI verilerini temizle
        foreach (Transform child in playerDataContainer)
        {
            Destroy(child.gameObject);
        }

        int index = 1;

        // Yeni verileri ekle
        foreach (var data in allPlayerData.Values)
        {
            GameObject playerDataObj;

            // Prefab se�iminde index'e g�re arka plan belirleniyor
            switch (index)
            {
                case 1:
                    playerDataObj = Instantiate(playerDataPrefab1, playerDataContainer);
                    break;
                case 2:
                    playerDataObj = Instantiate(playerDataPrefab2, playerDataContainer);
                    break;
                case 3:
                    playerDataObj = Instantiate(playerDataPrefab3, playerDataContainer);
                    break;
                case 4:
                    playerDataObj = Instantiate(playerDataPrefab4, playerDataContainer);
                    break;
                default:
                    playerDataObj = Instantiate(playerDataPrefab1, playerDataContainer); // Varsay�lan
                    break;
            }

            // Prefab i�erisindeki UI elemanlar�n� doldur
            playerDataObj.transform.Find("NicknameText").GetComponent<Text>().text = data.nickname;
            playerDataObj.transform.Find("CoinsText").GetComponent<Text>().text = data.coins.ToString();
            playerDataObj.transform.Find("ProfileImage").GetComponent<Image>().sprite = data.profileImage;

            index++;
        }

        // Game Over panelini g�ster
        gameOverPanel.SetActive(true);
    }
}
