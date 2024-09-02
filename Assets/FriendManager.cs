using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using Photon.Pun.Demo.Cockpit; // FriendListView ve FriendListCell i�in gerekli namespace

public class FriendManager : MonoBehaviourPunCallbacks
{
    public InputField friendRequestInput; // Arkada�l�k iste�i g�ndermek i�in input field
    public GameObject friendRequestPrefab; // Arkada�l�k iste�i i�in prefab
    public Transform friendRequestContainer; // �steklerin g�sterilece�i panel
    public FriendListView friendListView; // Photon'un sa�lad��� FriendListView bile�eni

    private List<string> friendsList = new List<string>(); // Takip edilecek arkada�lar�n listesi

    private void Start()
    {
        // Oyuncu giri� yapt���nda PlayFab'dan istekleri y�kle
        LoadFriendRequestsFromPlayFab();
    }

    // Arkada�l�k iste�i g�nderme
    public void SendFriendRequest()
    {
        string targetPlayerNickname = friendRequestInput.text; // Hedef oyuncunun ad�n� al�yoruz
        if (string.IsNullOrEmpty(targetPlayerNickname)) return;

        string senderNickname = PhotonNetwork.NickName; // G�nderen oyuncunun ad�n� al�yoruz

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ReceiveFriendRequest", RpcTarget.All, senderNickname, targetPlayerNickname);
    }


    // RPC ile arkada�l�k iste�i alma
    [PunRPC]
    public void ReceiveFriendRequest(string senderNickname, string targetNickname)
    {
        if (PhotonNetwork.NickName == targetNickname)
        {
            GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestContainer);

            // Prefaba do�ru g�nderici ad�n� atayal�m
            requestInstance.GetComponent<FriendRequestManager>().Initialize(senderNickname, this);
        }
    }

    // Arkada�l�k iste�ini kabul etme
    public void AcceptFriendRequest(string senderNickname)
    {
        // Arkada�� listeye ekleme i�lemi
        AddFriendToPhotonList(senderNickname);

        // �ste�i kabul ettikten sonra istek panelinden kald�rma i�lemi
        RemoveFriendRequest(senderNickname);

        // Arkada��n online durumu kontrol edilecek
        StartTrackingFriend(senderNickname);
    }

    private void AddFriendToPhotonList(string friendNickname)
    {
        if (friendListView == null)
        {
            Debug.LogError("FriendListView is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(friendNickname))
        {
            Debug.LogError("Friend nickname is null or empty!");
            return;
        }

        // Arkada�l�k listesine ekle
        friendsList.Add(friendNickname);

        // Photon'un Friend List View'�na arkada�� ekle
        friendListView.SetFriendDetails(new FriendListView.FriendDetail[]
        {
        new FriendListView.FriendDetail(friendNickname, friendNickname)
        });

        // Arkada��n online durumunu takip etmek i�in Photon'a bildirim g�nder
        PhotonNetwork.FindFriends(friendsList.ToArray());
    }


    // Arkada�l�k iste�ini reddetme veya kabul edilmeyen iste�i silme
    public void RejectFriendRequest(string senderNickname)
    {
        // �ste�i sil
        RemoveFriendRequest(senderNickname);
    }

    private void RemoveFriendRequest(string senderNickname)
    {
        // �stek listeden silinir ve PlayFab'dan kald�r�l�r
        foreach (Transform request in friendRequestContainer)
        {
            FriendRequestManager friendRequest = request.GetComponent<FriendRequestManager>();
            if (friendRequest.GetSenderNickname() == senderNickname)
            {
                Destroy(request.gameObject);
                RemoveFriendRequestFromPlayFab(senderNickname);
                break;
            }
        }
    }

    // PlayFab'a arkada�l�k iste�i kaydetme
    public void SaveFriendRequestToPlayFab(string senderId, string receiverId)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "FriendRequest_" + senderId, senderId }
            },
            Permission = UserDataPermission.Private
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("Friend request saved successfully."),
            error => Debug.LogError("Error saving friend request: " + error.GenerateErrorReport()));
    }

    // PlayFab'dan arkada�l�k isteklerini y�kleme
    public void LoadFriendRequestsFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.Count > 0)
                {
                    foreach (var entry in result.Data)
                    {
                        if (entry.Key.StartsWith("FriendRequest_"))
                        {
                            string senderId = entry.Value.Value;
                            // Bu iste�i UI'da g�sterin
                            ShowFriendRequest(senderId);
                        }
                    }
                }
            },
            error => Debug.LogError("Error loading friend requests: " + error.GenerateErrorReport()));
    }

    // PlayFab'da kaydedilen arkada�l�k iste�ini silme
    public void RemoveFriendRequestFromPlayFab(string senderId)
    {
        var updateDataRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "FriendRequest_" + senderId, null }
            }
        };

        PlayFabClientAPI.UpdateUserData(updateDataRequest,
            result => Debug.Log("Friend request removed successfully."),
            error => Debug.LogError("Error removing friend request: " + error.GenerateErrorReport()));
    }

    private void ShowFriendRequest(string senderId)
    {
        // Arkada�l�k iste�ini UI'da g�sterme i�lemini buraya ekleyin.
        GameObject requestInstance = Instantiate(friendRequestPrefab, friendRequestContainer);
        requestInstance.GetComponent<FriendRequestManager>().Initialize(senderId, this);
    }

    // Arkada��n durumunu takip etmeye ba�la
    private void StartTrackingFriend(string friendNickname)
    {
        if (!friendsList.Contains(friendNickname))
        {
            friendsList.Add(friendNickname);
            PhotonNetwork.FindFriends(friendsList.ToArray());
        }
    }

    // Arkada�lar�n durumlar�n� g�ncellemek i�in Photon'dan gelen geri �a�r�
    public override void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList)
    {
        // Photon Friend List View kullan�larak otomatik olarak g�ncellenecek
        friendListView.OnFriendListUpdate(friendList);
    }
}
