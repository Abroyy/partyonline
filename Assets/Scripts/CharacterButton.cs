using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Collections;

public class CharacterButton : MonoBehaviour
{
    public string characterPrefabName;
    public Transform spawnPoint;
    public int characterPrice;
    public int gemPrice; // Gem ile sat�n alma fiyat�
    [SerializeField] private Text buyText;
    [SerializeField] private Text gemText;
    public Button buyButton;
    public Button gemButton;
    private bool isPurchased;
    private bool isEquipped;

    public Button equipButton; // Equip butonunun referans�
    public Image tickImage; // Tik i�areti i�in Image bile�eni
    public Text uyar�Text;
    private bool isWarningTextVisible = false;

    private ButtonManager buttonManager;

    public bool IsEquipped
    {
        get => isEquipped;
        set
        {
            isEquipped = value;
            if (tickImage != null)
            {
                tickImage.gameObject.SetActive(isEquipped);
            }
        }
    }

    private void Start()
    {
        buttonManager = FindObjectOfType<ButtonManager>();

        bool isFirstCharacter = characterPrefabName == "Ya�l�";

        isPurchased = PlayerPrefs.GetInt(characterPrefabName + "_Purchased", 0) == 1 || isFirstCharacter;
        isEquipped = PlayerPrefs.GetInt(characterPrefabName + "_Equipped", 0) == 1;

        buyButton.gameObject.SetActive(!isPurchased);
        gemButton.gameObject.SetActive(!isPurchased);

        if (equipButton != null)
        {
            tickImage = equipButton.transform.Find("Image")?.GetComponent<Image>();
            if (tickImage != null)
            {
                tickImage.gameObject.SetActive(isEquipped);
            }
            else
            {
                Debug.LogError("TickImage not found.");
            }

            equipButton.gameObject.SetActive(isPurchased);
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }
        else
        {
            Debug.LogError("EquipButton not found.");
        }

        if (isFirstCharacter && !AnyOtherCharacterEquipped())
        {
            SpawnCharacter();
            EquipCharacter();
        }

        if (isEquipped)
        {
            buttonManager.SetLastEquippedCharacter(this);
        }
        else
        {
            // Check if this character is the last equipped character
            string lastEquippedCharacter = PlayerPrefs.GetString("LastEquippedCharacter", "");
            if (characterPrefabName == lastEquippedCharacter)
            {
                EquipCharacter();
            }
        }
    }




    private bool AnyOtherCharacterEquipped()
    {
        foreach (var button in buttonManager.characterButtons)
        {
            if (button != this && PlayerPrefs.GetInt(button.characterPrefabName + "_Equipped", 0) == 1)
            {
                return true;
            }
        }
        return false;
    }

    public void SpawnCharacter()
    {
        // �nceki karakteri sil
        foreach (Transform child in spawnPoint)
        {
            Destroy(child.gameObject);
        }

        GameObject characterPrefab = Resources.Load<GameObject>(characterPrefabName);
        if (characterPrefab != null)
        {
            // Yeni karakteri spawn et
            GameObject characterInstance = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            characterInstance.transform.rotation = Quaternion.Euler(0, 170, 0);
            characterInstance.transform.localScale = Vector3.one * 2.5f; 

            buyText.text = $"{characterPrice}";
            gemText.text = $"{gemPrice}";

            // Di�er butonlar� gizle
            buttonManager.UpdateActiveButton(this);

            isPurchased = PlayerPrefs.GetInt(characterPrefabName + "_Purchased", 0) == 1 || characterPrefabName == "Ya�l�";

            buyButton.gameObject.SetActive(!isPurchased);
            gemButton.gameObject.SetActive(!isPurchased);

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(isPurchased);
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(isEquipped);
                }
            }
        }
        else
        {
            Debug.LogError("Prefab not found: " + characterPrefabName);
        }
    }

    public void BuyCharacterWithCoins()
    {
        if (CoinManager.Instance.GetCurrentCoins() >= characterPrice)
        {
            CoinManager.Instance.SpendCoins(characterPrice);
            isPurchased = true;
            buyButton.gameObject.SetActive(false);
            gemButton.gameObject.SetActive(false);
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(true);
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(false);
                }
            }
            SavePurchaseStatus();
            UpdateCharacterPurchaseStatus(); // G�ncelleme
        }
        else
        {
            Debug.Log("Not enough coins to purchase the character.");
            uyar�Text.text = "Not enough coins to purchase the character!";
            if (!isWarningTextVisible)
            {
                StartCoroutine(ShowWarningTextForSeconds(5f));
            }
        }
    }
    public void BuyCharacterWithGems()
    {
        if (GemManager.Instance.GetCurrentGems() >= gemPrice)
        {
            GemManager.Instance.SpendGems(gemPrice);
            isPurchased = true;
            buyButton.gameObject.SetActive(false);
            gemButton.gameObject.SetActive(false);
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(true);
                if (tickImage != null)
                {
                    tickImage.gameObject.SetActive(false);
                }
            }
            SavePurchaseStatus();
            UpdateCharacterPurchaseStatus(); // G�ncelleme
        }
        else
        {
            Debug.Log("Not enough coins to purchase the character.");
            uyar�Text.text = "Not enough coins to purchase the character!";
            if (!isWarningTextVisible)
            {
                StartCoroutine(ShowWarningTextForSeconds(5f));
            }
        }
    }
    private IEnumerator ShowWarningTextForSeconds(float duration)
    {
        isWarningTextVisible = true; // Uyar� metnini g�r�n�r yap

        uyar�Text.gameObject.SetActive(true); // Uyar� metnini g�ster

        yield return new WaitForSeconds(duration); // Belirtilen s�re kadar bekle

        uyar�Text.gameObject.SetActive(false); // Uyar� metnini gizle
        isWarningTextVisible = false; // Uyar� metni g�r�n�rl�k durumunu s�f�rla
    }

    private void UpdateCharacterPurchaseStatus()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { characterPrefabName + "_Purchased", isPurchased ? "1" : "0" },
            { characterPrefabName + "_Equipped", isEquipped ? "1" : "0" },
            { "LastEquippedCharacter", PlayerPrefs.GetString("LastEquippedCharacter", "") },
            { "CurrentCoins", CoinManager.Instance.GetCurrentCoins().ToString() }, 
            { "CurrentGems", GemManager.Instance.GetCurrentGems().ToString() } 
        }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataUpdateSuccess, OnFailure);
    }

    private void OnDataUpdateSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Character purchase status updated successfully on PlayFab.");
    }
    private void OnPurchaseSuccess()
    {
        UpdateCharacterPurchaseStatus(); // Sat�n alma bilgilerini g�ncelle
    }

    private void OnFailure(PlayFabError error)
    {
        Debug.Log($"Error: {error.GenerateErrorReport()}");
    }

    private void SavePurchaseStatus()
    {
        PlayerPrefs.SetInt(characterPrefabName + "_Purchased", isPurchased ? 1 : 0);
        PlayerPrefs.SetInt(characterPrefabName + "_Equipped", isEquipped ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnEquipButtonClicked()
    {
        EquipCharacter();
        buttonManager.UpdateEquipButtons(this);
    }

    private void EquipCharacter()
    {
        // Di�er t�m karakterlerin equip durumunu s�f�rla
        foreach (var button in buttonManager.characterButtons)
        {
            if (button != this)
            {
                button.IsEquipped = false;
                PlayerPrefs.SetInt(button.characterPrefabName + "_Equipped", 0);
            }
        }

        IsEquipped = true; // Bu karakteri se�ildi olarak i�aretle
        PlayerPrefs.SetInt(characterPrefabName + "_Equipped", 1);
        PlayerPrefs.SetString("LastEquippedCharacter", characterPrefabName); // En son ekip edilen karakter
        PlayerPrefs.Save();

        UpdateCharacterPurchaseStatus(); // PlayFab'de g�ncelle
    }



}
