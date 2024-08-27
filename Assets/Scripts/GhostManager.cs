using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GhostManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    public Text[] rewardTexts;
    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSay�m geriSay�m;

    private List<Player> finishOrder = new List<Player>(); // Yakalananlar�n s�ralamas�
    private bool raceFinished = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();

        // Geri say�m olay�n� dinleyin
        GeriSay�m.OnGameOver += GameOver;

        // Geri say�m� ba�lat
        geriSay�m.StartCountdown();
    }

    private void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Player localPlayer = PhotonNetwork.LocalPlayer;

            // Oyuncunun ActorNumber'�na g�re spawn noktas�n� belirle
            int spawnPointIndex = localPlayer.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            // Son se�ilen karakter prefab ismini PlayerPrefs'ten al
            string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

            if (!string.IsNullOrEmpty(characterPrefabName))
            {
                // Karakter prefab'�n� PhotonNetwork.Instantiate ile instantiate et
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, spawnRotation);

                if (character != null)
                {
                    Debug.Log("Character successfully spawned.");
                }
                else
                {
                    Debug.LogError("Failed to instantiate character.");
                }
            }
            else
            {
                Debug.LogError("Character prefab not found in PlayerPrefs.");
            }
        }
    }


    private void SetPlayerProfileImage()
    {
        string playerImageName = PlayerPrefs.GetString("LastEquippedCharacter", "defaultProfileImage");
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties["profileImage"] = playerImageName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<PhotonView>().Owner;

            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player); // Yakalananlar� s�raya ekle
                UpdatePlayerUI(player, 0); // �lk yakaland���nda hen�z �d�l vermiyoruz, sadece UI g�ncelleme

                if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
                {
                    raceFinished = true;
                    geriSay�m.StopAllCoroutines(); // Geri say�m� durdur
                    GameOver(); // T�m oyuncular yakaland���nda oyunu bitir
                }
            }
        }
    }

    private void GameOver()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);

        // Yakalananlar i�in s�ral� �d�l ve yakalanmayanlar i�in sabit �d�l hesaplama
        int caughtReward = 250; // �lk yakalanan i�in ba�lang�� �d�l�
        List<Player> uncaughtPlayers = new List<Player>();
        List<Player> caughtPlayers = new List<Player>();

        // Yakalanan ve yakalanmayanlar� ay�r
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!finishOrder.Contains(player))
            {
                uncaughtPlayers.Add(player); // Yakalanmayanlar listesine ekle
            }
            else
            {
                caughtPlayers.Add(player); // Yakalananlar listesine ekle
            }
        }

        // Yakalanmayanlara 1000 �d�l ver
        foreach (Player player in uncaughtPlayers)
        {
            UpdatePlayerUI(player, 1000); // UI g�ncelleme (1000 �d�l)
            CoinManager.Instance.AddCoins(1000); // 1000 �d�l ekle
        }

        // Yakalananlara s�ral� �d�l ver
        for (int i = 0; i < caughtPlayers.Count; i++)
        {
            Player caughtPlayer = caughtPlayers[i];
            UpdatePlayerUI(caughtPlayer, caughtReward); // Yakalananlar i�in UI g�ncelleme
            CoinManager.Instance.AddCoins(caughtReward); // S�raya g�re �d�l ekle
            caughtReward += 250; // Her s�radaki oyuncuya �d�l 250 artar
        }
    }

    private void UpdatePlayerUI(Player player, int reward)
    {
        int playerIndex = PhotonNetwork.PlayerList.ToList().IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player);
            rewardTexts[playerIndex].text = $"{reward}"; // �d�l metnini g�ncelle
            nickNames[playerIndex].text = player.NickName; // Oyuncunun ismini g�ncelle
        }
    }

    private Sprite GetProfileSprite(Player player)
    {
        if (player.CustomProperties.TryGetValue("profileImage", out object playerImageName))
        {
            Sprite profileSprite = Resources.Load<Sprite>("ProfileImages/" + playerImageName.ToString());

            if (profileSprite == null)
            {
                profileSprite = Resources.Load<Sprite>("ProfileImages/defaultProfileImage");
            }

            return profileSprite;
        }
        else
        {
            return Resources.Load<Sprite>("ProfileImages/defaultProfileImage");
        }
    }

    private void OnDestroy()
    {
        GeriSay�m.OnGameOver -= GameOver;
    }
}
