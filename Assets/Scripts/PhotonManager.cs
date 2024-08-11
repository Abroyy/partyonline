using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private const string FirstTimeKey = "FirstTime";
    private const string LastEquippedCharacterKey = "LastEquippedCharacter";
    private string[] spawnPoints = { "SpawnPoint1", "SpawnPoint2", "SpawnPoint3", "SpawnPoint4" };
    private Dictionary<int, string> playerSpawnPoints = new Dictionary<int, string>();
    private HashSet<string> occupiedSpawnPoints = new HashSet<string>();

    public Button playButton;
    public GameObject loadingPanel;
    public Text statusText;  // Ma� arama ve geri say�m mesaj� i�in Text
    private bool isCountingDown = false;
    public Button leaveRoomButton;
    public Button cosmeticsButton;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected to Photon.");
        }

        if (playButton != null)
        {
            playButton.interactable = true;
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("Play button is not assigned.");
        }

        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
            leaveRoomButton.gameObject.SetActive(false); // Ba�lang��ta butonu pasif yap
        }
        else
        {
            Debug.LogError("Leave room button is not assigned.");
        }
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            playButton.interactable = false;
            StartCountdown();
        }
    }

    private void OnPlayButtonClicked()
    {
        Play();
    }

    public void Play()
    {
        if (PlayerPrefs.GetInt(FirstTimeKey, 1) == 1)
        {
            PlayerPrefs.SetInt(FirstTimeKey, 0);
            ShowCharacterShop();
        }
        else
        {
            ConnectToLobby();
        }
    }

    private void OnLeaveRoomButtonClicked()
    {
        LeaveRoom();
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            Debug.LogWarning("Not currently in a room to leave.");
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room successfully.");
        leaveRoomButton.gameObject.SetActive(false); // Odadan ��k�ld���nda butonu pasif yap
        playButton.interactable = true;
        statusText.text = "You have left the room."; // Oyuncuya odadan ��kt���n� bildir
        statusText.gameObject.SetActive(true); // Mesaj� g�ster

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = true; // Odadan ��k�ld���nda cosmetics butonunu tekrar aktif yap
        }

        StartCoroutine(HideStatusTextAfterDelay(1.5f));

    }

    private void ShowCharacterShop()
    {
        SceneManager.LoadScene("CharacterShop");
    }

    public void ConnectToLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
            statusText.text = "Searching for matchmaking...";
            statusText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Photon is not connected.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room.");
        leaveRoomButton.gameObject.SetActive(true);
        playButton.interactable = false;

        if (cosmeticsButton != null)
        {
            cosmeticsButton.interactable = false;
        }
        else
        {
            Debug.LogError("Cosmetics button is not assigned.");
        }
        string characterPrefabName = PlayerPrefs.GetString(LastEquippedCharacterKey, "DefaultCharacter");

        string availableSpawnPointName = GetAvailableSpawnPoint();
        if (!string.IsNullOrEmpty(characterPrefabName) && availableSpawnPointName != null)
        {
            GameObject spawnPoint = GameObject.Find(availableSpawnPointName);
            if (spawnPoint != null)
            {
                Vector3 spawnPosition = spawnPoint.transform.position;
                GameObject character = PhotonNetwork.Instantiate(characterPrefabName, spawnPosition, Quaternion.identity);
                if (character != null)
                {
                    character.transform.rotation = Quaternion.Euler(0, 10, 0);

                    // Oyuncu ile spawn noktas�n� e�le�tir
                    playerSpawnPoints[PhotonNetwork.LocalPlayer.ActorNumber] = availableSpawnPointName;
                    occupiedSpawnPoints.Add(availableSpawnPointName);  // Spawn noktas�n� me�gul olarak i�aretle
                }
                else
                {
                    Debug.LogError("Failed to instantiate character.");
                }
            }
        }
        else
        {
            Debug.LogError("Character prefab name is empty or no available spawn points.");
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} has left the room.");

        // E�er oyuncu ayr�ld�ysa, onun spawn noktas�n� bo�alt
        if (playerSpawnPoints.ContainsKey(otherPlayer.ActorNumber))
        {
            string spawnPointName = playerSpawnPoints[otherPlayer.ActorNumber];

            if (spawnPointName != null && occupiedSpawnPoints.Contains(spawnPointName))
            {
                occupiedSpawnPoints.Remove(spawnPointName);  // Spawn noktas�n� tekrar kullan�labilir hale getir
                playerSpawnPoints.Remove(otherPlayer.ActorNumber);
                Debug.Log($"Spawn point '{spawnPointName}' is now available.");
            }
        }
    }
    private string GetAvailableSpawnPoint()
    {
        List<string> availableSpawnPoints = new List<string>(spawnPoints);

        // E�er oyuncu odan�n kurucusu de�ilse, ilk spawn noktas�n� kullan�lamaz yap
        if (!PhotonNetwork.IsMasterClient)
        {
            availableSpawnPoints.RemoveAt(0);  // �lk spawn noktas�n� ��kar
        }

        foreach (string spawnPointName in availableSpawnPoints)
        {
            if (!occupiedSpawnPoints.Contains(spawnPointName))
            {
                Debug.Log($"Spawn point '{spawnPointName}' is available.");
                return spawnPointName;
            }
        }

        Debug.LogError("No available spawn points.");
        return null;
    }

    private void StartCountdown()
    {
        if (!isCountingDown)
        {
            isCountingDown = true;
            StartCoroutine(CountdownCoroutine(10));
        }
    }

    private IEnumerator HideStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        statusText.gameObject.SetActive(false); // Mesaj� gizle
    }

    void ShowLoadingPanel()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Loading panel is not assigned.");
        }
    }

    private IEnumerator CountdownCoroutine(int seconds)
    {
        while (seconds > 0)
        {
            statusText.text = $"Match found! Starting in {seconds}...";
            yield return new WaitForSeconds(1);
            seconds--;
        }

        ShowLoadingPanel();

        // Bekleme s�resi eklemek gerekebilir (opsiyonel)
        yield return new WaitForSeconds(3);

        // Sahneyi y�kle
        PhotonNetwork.LoadLevel("TrapPG");
    }
    private string FindNearestSpawnPoint(Vector3 position)
    {
        GameObject nearestSpawnPoint = null;
        float minDistance = float.MaxValue;

        foreach (string spawnPointName in spawnPoints)
        {
            GameObject spawnPoint = GameObject.Find(spawnPointName);
            if (spawnPoint != null)
            {
                float distance = Vector3.Distance(position, spawnPoint.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSpawnPoint = spawnPoint;
                }
            }
        }

        return nearestSpawnPoint != null ? nearestSpawnPoint.name : null;
    }
}
