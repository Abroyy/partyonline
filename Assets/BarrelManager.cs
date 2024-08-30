using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class BarrelManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public Image[] profileImages;

    // Score system
    public Text[] playerScoreTexts;
    public float scoreIncrement = 1f;

    public Text[] nickNames;
    public GameObject gameOverPanel;
    public GeriSay�m geriSay�m;

    public Text[] gameOverScoreTexts;
    public Image[] gameOverProfileImages;
    public Text[] gameOverNickNames;

    private List<Player> frozenPlayers = new List<Player>();
    private bool raceFinished = false;
    private bool scoreCountingStarted = false;

    private void Start()
    {
        ResetPlayerScores();
        gameOverPanel.SetActive(false);
        SpawnPlayer();
        SetPlayerProfileImage();
        GeriSay�m.OnGameOver += GameOver_RPC;
        geriSay�m.StartCountdown();

        StartCoroutine(StartScoreCountingAfterDelay(10f));
    }

    private IEnumerator StartScoreCountingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        scoreCountingStarted = true;
        StartCoroutine(UpdateScores());
    }

    private IEnumerator UpdateScores()
    {
        while (!raceFinished && PhotonNetwork.IsMasterClient)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!frozenPlayers.Contains(player))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        float currentScore = GetPlayerScore(player);
                        currentScore += 1f; 
                        SetPlayerScore(player, currentScore);
                    }
                }
            }

            photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);

            yield return new WaitForSeconds(0.1f);
        }
    }


    [PunRPC]
    private void UpdateScoreUI_RPC()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            playerScoreTexts[i].text = GetPlayerScore(player).ToString("F0");
        }
    }
    private void ResetPlayerScores()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            SetPlayerScore(player, 0f);
        }

        photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);
    }

    private float GetPlayerScore(Player player)
    {
        if (player.CustomProperties.TryGetValue("PlayerScore", out object score))
        {
            return (float)score;
        }
        return 0f;
    }

    private void SetPlayerScore(Player player, float score)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerScore", score }
        };
        player.SetCustomProperties(playerProperties);
    }

    public void OnPlayerDeath(Player player)
    {
        if (!frozenPlayers.Contains(player))
        {
            frozenPlayers.Add(player);
            photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All);

            CheckForGameOver();
        }
    }
    private void CheckForGameOver()
    {
        if (frozenPlayers.Count == PhotonNetwork.PlayerList.Length)
        {
            StartCoroutine(DelayedGameOver());
        }
    }
    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(2f);
        photonView.RPC("GameOver_RPC", RpcTarget.All);
    }
    private void SpawnPlayer()
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;

        int spawnPointIndex = localPlayer.ActorNumber % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[spawnPointIndex];
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        string characterPrefabName = PlayerPrefs.GetString("LastEquippedCharacter", "DefaultCharacter");

        if (!string.IsNullOrEmpty(characterPrefabName))
        {
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

    private void SetPlayerProfileImage()
    {
        string playerImageName = PlayerPrefs.GetString("LastEquippedCharacter");
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "profileImage", playerImageName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    private void GameOver_RPC()
    {
        raceFinished = true;
        gameOverPanel.SetActive(true);
        UpdateGameOverScores();
    }

    private void UpdateGameOverScores()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            gameOverScoreTexts[i].text = GetPlayerScore(player).ToString("F0");
            gameOverProfileImages[i].sprite = GetProfileSprite(player);
            gameOverNickNames[i].text = player.NickName;
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

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("New MasterClient assigned: " + newMasterClient.NickName);

        // E�er oyun hen�z bitmediyse, yeni MasterClient g�revleri devral�r
        if (!raceFinished && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Debug.Log("I am the new MasterClient, taking over responsibilities.");
            StartCoroutine(StartScoreCountingAfterDelay(0f));
        }
    }

    private void OnDestroy()
    {
        GeriSay�m.OnGameOver -= GameOver_RPC;
    }
}
