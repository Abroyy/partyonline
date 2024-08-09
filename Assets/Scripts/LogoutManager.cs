using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using Photon.Pun;

public class LogoutManager : MonoBehaviour
{
    public void Logout()
    {
        // PlayFab'den ��k�� yapma
        PlayFabClientAPI.ForgetAllCredentials();

        // Photon'dan ��k�� yapma
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        // Kullan�c� verilerini temizleme
        PlayerPrefs.DeleteAll();

        // Giri� ekran�na y�nlendirme
        SceneManager.LoadScene("Login"); // LoginScene ad�ndaki sahneye y�nlendiriyor
    }
}
