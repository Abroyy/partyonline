using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class AddFriend : MonoBehaviour
{
    // Arkada� ekleme
    public InputField friendInputField;

    // Arkada� iste�i kabul ve reddetme
    public GameObject friendRequestPrefab;
    public Transform requestParentPanel;

    // Arkada� listesi i�in
    public GameObject friendListItemPrefab;
    public Transform friendsListContainer;

    private string playerDisplayName;

    private void Start()
    {
        // Oyuncunun profil bilgilerini al ve DisplayName'i sakla
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), result =>
        {
            playerDisplayName = result.AccountInfo.TitleInfo.DisplayName;
            Debug.Log("Player DisplayName: " + playerDisplayName);

            // Arkada�l�k isteklerini kontrol et
            CheckFriendRequests();
        }, error =>
        {
            Debug.LogError("Error getting account info: " + error.GenerateErrorReport());
        });
    }

    // 1. Arkada�l�k iste�i g�nderme
    public void AddFriendByDisplayName()
    {
        string friendDisplayName = friendInputField.text;

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { TitleDisplayName = friendDisplayName }, result =>
        {
            string targetPlayerId = result.AccountInfo.PlayFabId;

            var request = new AddFriendRequest
            {
                FriendPlayFabId = targetPlayerId
            };

            PlayFabClientAPI.AddFriend(request, addFriendResult =>
            {
                Debug.Log("Friend request sent successfully to " + friendDisplayName);

                // Arkada� listesi periyodik olarak kontrol edilmeye ba�lanabilir
                StartCoroutine(CheckIfFriendAccepted(targetPlayerId));

            }, error =>
            {
                Debug.LogError("Error sending friend request: " + error.GenerateErrorReport());
            });

        }, error =>
        {
            Debug.LogError("Error retrieving account info: " + error.GenerateErrorReport());
        });
    }
    private IEnumerator CheckIfFriendAccepted(string friendPlayFabId)
    {
        while (true)
        {
            // Belirli bir s�re bekliyoruz (�rne�in 5 saniye)
            yield return new WaitForSeconds(5f);

            // Arkada� listesini kontrol ediyoruz
            bool friendFound = false;
            PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(), result =>
            {
                foreach (var friend in result.Friends)
                {
                    if (friend.FriendPlayFabId == friendPlayFabId)
                    {
                        Debug.Log("Friend request accepted by: " + friend.TitleDisplayName);
                        friendFound = true;

                        // E�er arkada�l�k kabul edildiyse, her iki taraf da UI'lar�n� g�nceller
                        AddFriendToFriendsList(friend.TitleDisplayName);
                        break;
                    }
                }

                if (friendFound)
                {
                    StopCoroutine("CheckIfFriendAccepted"); // Kontrol i�lemi sonland�r�l�r
                }

            }, error =>
            {
                Debug.LogError("Error retrieving friends list: " + error.GenerateErrorReport());
            });

            if (friendFound)
            {
                yield break; // E�er arkada� bulunduysa, d�ng�den ��k�yoruz
            }
        }
    }
    // 2. Arkada�l�k isteklerini kontrol etme
    public void CheckFriendRequests()
    {
        var request = new GetFriendsListRequest(); // Sadece bo� bir istek olu�turun

        PlayFabClientAPI.GetFriendsList(request, result =>
        {
            if (result.Friends != null && result.Friends.Count > 0)
            {
                foreach (var friend in result.Friends)
                {
                    // Arkada�l�k iste�i veya arkada� bilgilerini burada i�leyebilirsiniz
                    Debug.Log("Friend: " + friend.TitleDisplayName);
                    OnFriendRequestReceived(friend.TitleDisplayName, friend.FriendPlayFabId);
                }
            }
            else
            {
                Debug.Log("No friends or pending friend requests found.");
            }
        }, error =>
        {
            Debug.LogError("Error retrieving friends list: " + error.GenerateErrorReport());
        });
    }


    // 3. Arkada�l�k iste�i al�nd���nda UI g�ncellemesi
    public void OnFriendRequestReceived(string friendDisplayName, string friendPlayFabId)
    {
        // Prefab olu�tur
        GameObject newRequest = Instantiate(friendRequestPrefab, requestParentPanel);

        // Prefab'�n i�indeki Text ve Buton bile�enlerine eri�
        Text displayNameText = newRequest.transform.Find("FriendNameText").GetComponent<Text>();
        Button acceptButton = newRequest.transform.Find("AcceptButton").GetComponent<Button>();
        Button declineButton = newRequest.transform.Find("DeclineButton").GetComponent<Button>();

        // Oyuncunun ismini Text bile�enine atay�n
        displayNameText.text = friendDisplayName;

        // Butonlara i�lev ekle
        acceptButton.onClick.RemoveAllListeners(); // Mevcut listener'lar� kald�r
        acceptButton.onClick.AddListener(() => AcceptFriendRequest(friendPlayFabId, newRequest));

        declineButton.onClick.RemoveAllListeners(); // Mevcut listener'lar� kald�r
        declineButton.onClick.AddListener(() => DeclineFriendRequest(newRequest));
    }


    // 4. Arkada�l�k iste�ini kabul etme
    public void AcceptFriendRequest(string friendPlayFabId, GameObject requestUI)
    {
        Debug.Log("Accept button clicked for: " + friendPlayFabId);

        PlayFabClientAPI.AddFriend(new AddFriendRequest { FriendPlayFabId = friendPlayFabId }, result =>
        {
            Debug.Log("Friend request accepted for " + friendPlayFabId);

            // Arkada� listeye eklendi�inde, arkada� listesi UI'�na ekle
            AddFriendToFriendsList(friendPlayFabId);

            // Arkada�l�k iste�i UI'sini kald�r
            Destroy(requestUI);
        }, error =>
        {
            Debug.LogError("Error accepting friend request: " + error.GenerateErrorReport());
            Destroy(requestUI);
        });
    }

    // 5. Arkada�l�k iste�ini reddetme
    public void DeclineFriendRequest(GameObject requestUI)
    {
        // Arkada�l�k iste�i UI'sini kald�r
        Destroy(requestUI);
    }

    // 6. Arkada�� arkada� listesi UI'�na ekleme
    private void AddFriendToFriendsList(string friendDisplayName)
    {
        GameObject newFriendItem = Instantiate(friendListItemPrefab, friendsListContainer);
        Text friendNameText = newFriendItem.transform.Find("FriendNameText").GetComponent<Text>();
        friendNameText.text = friendDisplayName;
        Debug.Log("Friend " + friendDisplayName + " added to friends list.");
    }

    // 7. Arkada� listesini g�ncelleme (iste�e ba�l�)
    public void GetFriendsList()
    {
        var request = new GetFriendsListRequest();

        PlayFabClientAPI.GetFriendsList(request, result =>
        {
            if (result.Friends != null && result.Friends.Count > 0)
            {
                foreach (var friend in result.Friends)
                {
                    Debug.Log("Friend: " + friend.TitleDisplayName);
                    AddFriendToFriendsList(friend.TitleDisplayName);
                }
            }
            else
            {
                Debug.Log("No friends found.");
            }
        }, error =>
        {
            Debug.LogError("Error retrieving friends list: " + error.GenerateErrorReport());
        });
    }
    // 8. Lobiye davet g�nderme (iste�e ba�l�)
    public void InviteFriendToLobby(string friendDisplayName)
    {
        // RPC veya Photon mesajla�ma sistemi kullanarak arkada��n�za bir davet g�nderin
        Debug.Log("Invite sent to: " + friendDisplayName);
    }
}
