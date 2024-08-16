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
        public int rank; // Oyuncunun biti� �izgisinden ge�i� s�ras�
    }

    private List<string> finishOrder = new List<string>(); // Biti� s�ras�

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    [PunRPC]
    public void SyncPlayerData(string playerId, int coins, string profileImageName, int rank)
    {
        // PlayerPrefs'ten nickname'i al
        string nickname = PlayerPrefs.GetString("DISPLAYNAME", "Guest");

        Sprite profileImage = Resources.Load<Sprite>("ProfileImages/" + profileImageName);

        PlayerData playerData = new PlayerData
        {
            nickname = nickname,
            coins = coins,
            profileImage = profileImage,
            rank = rank
        };

        if (!allPlayerData.ContainsKey(playerId))
        {
            allPlayerData.Add(playerId, playerData);
        }

        // Update UI
        UpdateGameOverPanel();
    }

    public void OnPlayerFinish(string playerId)
    {
        if (finishOrder.Contains(playerId)) return; // Oyuncu zaten biti� �izgisini ge�tiyse tekrar eklenmez

        finishOrder.Add(playerId);

        int earnedCoins = CalculateCoins(finishOrder.Count); // Biti� s�ras�na g�re coins hesapla
        string profileImageName = "defaultProfileImage"; // Profil resminin ad�n� buradan belirleyin

        photonView.RPC("SyncPlayerData", RpcTarget.All, playerId, earnedCoins, profileImageName, finishOrder.Count);

        // Oyuncunun kazand��� coinleri g�ncelle
        CoinManager.Instance.AddCoins(earnedCoins);

        // E�er t�m oyuncular biti� �izgisine ula�t�ysa, oyunu bitir
        if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
        {
            OnGameOver();
        }
    }

    private int CalculateCoins(int rank)
    {
        switch (rank)
        {
            case 1: return 3000;
            case 2: return 1500;
            case 3: return 1000;
            case 4: return 500;
            default: return 0; // 4. s�radan sonras� veya hi� biti� �izgisine ula�amayanlar i�in 0 coins
        }
    }

    void UpdateGameOverPanel()
    {
        // Eski UI verilerini temizle
        foreach (Transform child in playerDataContainer)
        {
            Destroy(child.gameObject);
        }

        // Yeni verileri ekle
        foreach (var data in allPlayerData.Values)
        {
            GameObject playerDataObj;

            // Prefab se�iminde oyuncunun s�ras�na g�re arka plan belirleniyor
            switch (data.rank)
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
            Text nicknameText = playerDataObj.transform.Find("NicknameText").GetComponent<Text>();
            Text coinsText = playerDataObj.transform.Find("CoinsText").GetComponent<Text>();
            Image profileImage = playerDataObj.transform.Find("ProfileImage").GetComponent<Image>();

            nicknameText.text = data.nickname;
            coinsText.text = data.coins.ToString();
            profileImage.sprite = data.profileImage;
        }

        // Game Over panelini g�ster
        gameOverPanel.SetActive(true);
    }

    public void OnGameOver()
    {
        // Biti� �izgisine ula�amayan oyuncular i�in coins de�erini 0 olarak belirleyin
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string playerId = player.UserId; // Oyuncu ID'sini al�n
            if (!finishOrder.Contains(playerId))
            {
                photonView.RPC("SyncPlayerData", RpcTarget.All, playerId, 0, "defaultProfileImage", 0);
            }
        }

        // Game Over panelini g�ncelle ve g�ster
        UpdateGameOverPanel();
    }
}
