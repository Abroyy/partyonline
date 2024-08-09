using UnityEngine;
using UnityEngine.UI;

public class MainMenuCoin : MonoBehaviour
{
    public Text coinText; // Ana men�deki coin miktar�n� g�steren Text
    public Text gemText; // Ana men�deki gem miktar�n� g�steren Text
    public GameObject announcementPanel;
    public Button okButton;

    private void Start()
    {
        CoinManager.Instance.SetCoinText(coinText);
        GemManager.Instance.SetGemText(gemText);

        if (PlayerPrefs.GetInt("FirstTimeRegistration", 0) == 1)
        {
            // Paneli a�
            if (announcementPanel != null)
            {
                announcementPanel.SetActive(true);
            }

            // �lk kayd�n tamamland���n� belirt
            PlayerPrefs.SetInt("FirstTimeRegistration", 0);
            PlayerPrefs.Save();
        }
        else
        {
            // Daha �nce kay�t olmu�sa paneli gizle
            if (announcementPanel != null)
            {
                announcementPanel.SetActive(false);
            }
        }

        if (okButton != null)
        {
            okButton.onClick.AddListener(CloseAnnouncementPanel);
        }
        else
        {
            Debug.LogError("OK Button not assigned.");
        }
    }
    private void CloseAnnouncementPanel()
    {
        if (announcementPanel != null)
        {
            announcementPanel.SetActive(false);
        }
    }
}
