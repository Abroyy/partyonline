using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int currentCoins;
    public Text coinText; // UI'daki coin miktar�n� g�steren Text

    private void Awake()
    {
        // Singleton tasar�m deseni
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Oyun sahnesi de�i�se bile bu objeyi yok etme
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentCoins = PlayerPrefs.GetInt("Coins", 1000000);
        UpdateCoinUI();
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        SaveCoins();
        UpdateCoinUI();
    }

    public void SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            SaveCoins();
            UpdateCoinUI();
        }
        else
        {
            Debug.Log("Not enough coins.");
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
        }
    }
    public void SetCurrentCoins(int coins)
    {
        currentCoins = coins;
        UpdateCoinUI(); // UI'yi g�ncelle
    }


    public void SetCoinText(Text newCoinText)
    {
        coinText = newCoinText;
        UpdateCoinUI();
    }
}
