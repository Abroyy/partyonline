using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterShop : MonoBehaviour
{
    public Text coinText; // Karakter ma�azas�nda coin miktar�n� g�steren Text
    public Text gemText; // Karakter ma�azas�nda gem miktar�n� g�steren Text

    private void Start()
    {
        CoinManager.Instance.SetCoinText(coinText);
        GemManager.Instance.SetGemText(gemText);
    }
}
