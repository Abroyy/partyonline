using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;
    public Text[] rewardTexts;
    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSay�m geriSay�m; // Geri say�m scripti

    private List<Player> finishOrder = new List<Player>();
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
        if (raceFinished)
            return;

        if (other.CompareTag("Player")) // Finish �izgisinden ge�en oyunculara g�re i�lem yap�l�r
        {
            Player player = other.GetComponent<PhotonView>().Owner;

            // E�er oyuncu zaten finish s�ras�na eklenmi�se, tekrar ekleme
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);

                // Oyuncu s�ras�n� belirleme ve �d�lleri verme
                UpdatePlayerUI(player);

                // E�er t�m oyuncular bitirdiyse yar��� sonland�r
                if (finishOrder.Count == PhotonNetwork.PlayerList.Length)
                {
                    raceFinished = true;
                    geriSay�m.StopAllCoroutines(); // Geri say�m� durdur
                    GameOver();
                }
            }
        }
    }

    private void UpdatePlayerUI(Player player)
    {
        int playerIndex = finishOrder.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < profileImages.Length)
        {
            profileImages[playerIndex].sprite = GetProfileSprite(player);

            // �lk oyuncuya 1000 �d�l ver, sonrakilere s�ras�yla 250 daha az ver
            int reward = Mathf.Max(1000 - (playerIndex * 250), 0); // En d���k �d�l 0 olmal�

            rewardTexts[playerIndex].text = $"{reward}";
            CoinManager.Instance.AddCoins(reward);

            nickNames[playerIndex].text = player.NickName;
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

    private void GameOver()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);

        // Yar��� bitirmeyen oyuncular� da UI'da g�stermek, ancak �d�l vermemek i�in
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);  // Yar��� bitirmeyen oyuncular� listeye ekle
                UpdatePlayerUI(player);  // Ancak bunlara s�f�r �d�l ver
            }
        }
    }

    private void OnDestroy()
    {
        GeriSay�m.OnGameOver -= GameOver;
    }
}
