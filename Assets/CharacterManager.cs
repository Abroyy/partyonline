using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public GameObject[] characterPrefabs; // Her karakterin 3D prefabs
    public Transform previewPosition; // Karakter �nizlemesinin yap�laca�� pozisyon
    public Button purchaseButton; // Sat�n Al butonu
    public Button selectButton; // Se� butonu
    public Text purchaseButtonText; // Sat�n Al butonunun text bile�eni
    public float[] characterPrices; // Karakter fiyatlar�
    public Image[] catalogCharacterButtons; // Katalogdaki karakter butonlar�
    private GameObject currentPreview; // �u anda g�sterilen karakter �nizleme objesi
    public Text warningText;
    public GameObject itemButtonPrefab; // Her item i�in kullan�lacak buton prefab�
    public Transform itemsContainer;
    void Start()
    {
        OfferFirstCharacterForFree(); // �lk karakteri �cretsiz sunma kontrol�

        // En son se�ilen karakteri al
        int lastSelectedCharacterIndex = PlayerPrefs.GetInt("LastEquippedCharacter", 0); // E�er hi� se�im yap�lmad�ysa default olarak 0 d�ner

        // En son se�ilen karakteri instantiate et
        ShowCharacterPreview(lastSelectedCharacterIndex);

        // Tick i�aretlerini g�ncelle
        UpdateCatalogUI(lastSelectedCharacterIndex);
        CheckAndUpdateLockedImages();
    }
    public void ShowCharacterPreview(int characterIndex)
    {
        // Mevcut �nizleme objesini temizle
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        // Yeni karakter prefab�n� olu�tur ve do�ru y�n, �l�ek ve pozisyonda ayarla
        currentPreview = Instantiate(characterPrefabs[characterIndex], previewPosition.position, Quaternion.Euler(0, 165, 0)); // Rotasyon ayar�

        // Karakterin boyutunu ayarlamak i�in localScale'i kullanabilirsin
        currentPreview.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f); // Boyut ayar�
        // Butonlar� g�ncelle
        UpdateButtons(characterIndex);
        InstantiateCharacterItems(characterIndex);
        // Sat�n Al Butonu ��in Dinamik Ba�lama
        purchaseButton.onClick.RemoveAllListeners(); // �nceki t�m dinleyicileri kald�r
        purchaseButton.onClick.AddListener(() => TryPurchaseCharacter(characterIndex)); // Sat�n al butonuna karakter index'ini ba�la

        // Se� Butonu ��in Dinamik Ba�lama
        selectButton.onClick.RemoveAllListeners(); // �nceki t�m dinleyicileri kald�r
        selectButton.onClick.AddListener(() => SelectCharacter(characterIndex)); // Se� butonuna karakter index'ini ba�la
    }
    private void InstantiateCharacterItems(int characterIndex)
    {
        // �nce paneldeki mevcut item butonlar�n� temizle
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Karakter alt�ndaki t�m itemleri bulmak i�in GetComponentsInChildren kullan
        Item[] items = currentPreview.GetComponentsInChildren<Item>(true); // T�m child'lardaki item bile�enlerini al

        if (items.Length == 0)
        {
            Debug.LogError("No items found on character!");
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            GameObject itemButton = Instantiate(itemButtonPrefab, itemsContainer);

            int currentItemIndex = i; // Lambda fonksiyonu i�in indexi yakala

            // �lk item varsay�lan olarak aktif, di�erleri deaktif
            if (i == 0)
            {
                items[i].gameObject.SetActive(true);
            }
            else
            {
                items[i].gameObject.SetActive(false);
            }

            // Butonun g�r�nt�s�n� ve ismini ayarla (opsiyonel)
            itemButton.transform.Find("ItemIcon").GetComponent<Image>().sprite = items[i].itemIcon;
            itemButton.transform.Find("ItemName").GetComponent<Text>().text = items[i].itemName;

            // Butona itemi aktifle�tirecek dinamik listener ekle
            itemButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ActivateItem(items, currentItemIndex); // �lgili itemi aktifle�tir
            });
        }
    }

    private void ActivateItem(Item[] items, int itemIndex)
    {
        // T�m itemleri deaktif et, sadece se�ilen itemi aktif et
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(i == itemIndex);
        }
    }

    public void TryPurchaseCharacter(int characterIndex)
    {
        int characterPrice = (int)characterPrices[characterIndex];

        // CoinManager'dan mevcut coin miktar�n� al
        int currentCoins = CoinManager.Instance.GetCurrentCoins();

        if (currentCoins >= characterPrice)
        {
            // Yeterli coin varsa karakteri sat�n al
            CoinManager.Instance.SpendCoins(characterPrice);
            PurchaseCharacter(characterIndex);
        }
        else
        {
            // Yetersiz coin uyar�s� g�ster
            if (warningText != null)
            {
                warningText.text = "Not Enough Coin!";
            }
        }
    }

    // Butonlar�n Durumunu G�ncelleme Fonksiyonu
    public void UpdateButtons(int characterIndex)
    {
        // E�er karakter sat�n al�nd�ysa
        if (PlayerPrefs.GetInt("CharacterPurchased_" + characterIndex) == 1)
        {
            purchaseButton.gameObject.SetActive(false); // Sat�n Al butonunu gizle
            selectButton.gameObject.SetActive(true); // Se� butonunu g�ster
        }
        else // Karakter sat�n al�nmad�ysa
        {
            purchaseButton.gameObject.SetActive(true); // Sat�n Al butonunu g�ster
            selectButton.gameObject.SetActive(false); // Se� butonunu gizle
            purchaseButtonText.text = characterPrices[characterIndex].ToString("F0") + " Coins"; // Sat�n Al butonuna fiyat yazd�r
        }
    }

    // Karakter Sat�n Alma Fonksiyonu
    // Karakter Sat�n Alma Fonksiyonu
    public void PurchaseCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt("CharacterPurchased_" + characterIndex, 1); // Karakteri sat�n al�nd� olarak i�aretle
        PlayerPrefs.SetInt("CharacterLocked_" + characterIndex, 0); // Karakterin kilidini a��k olarak i�aretle (0 = kilit a��k)

        UpdateButtons(characterIndex); // Butonlar� g�ncelle

        // Sat�n al�nan karakterin LockedImage'ini gizle
        HideLockedImage(characterIndex);
    }

    // LockedImage'i Gizleme Fonksiyonu
    private void HideLockedImage(int characterIndex)
    {
        // Karakterin ilgili butonunu al
        Transform characterButton = catalogCharacterButtons[characterIndex].transform;

        // LockedImage bile�enini bul ve gizle
        Transform lockedImageTransform = characterButton.Find("Locked");

        if (lockedImageTransform != null)
        {
            lockedImageTransform.gameObject.SetActive(false); // LockedImage'i gizle
        }
        else
        {
            Debug.LogWarning("LockedImage not found for character " + characterIndex);
        }
    }

    // Oyun ba�lad���nda sat�n al�nan karakterlerin LockedImage'lerini kontrol et
    public void CheckAndUpdateLockedImages()
    {
        for (int i = 0; i < catalogCharacterButtons.Length; i++)
        {
            // E�er karakter sat�n al�nd�ysa (kilidi a��k olarak i�aretlendiyse)
            if (PlayerPrefs.GetInt("CharacterLocked_" + i, 1) == 0) // 1 = kilitli, 0 = kilit a��k
            {
                HideLockedImage(i); // Karakterin LockedImage'ini gizle
            }
        }
    }

    // Karakter Se�me Fonksiyonu
    public void SelectCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex); // Se�ilen karakteri kaydet
        PlayerPrefs.SetString("LastEquippedCharacter", characterPrefabs[characterIndex].name); // Karakterin prefab ad�n� kaydet
        PlayerPrefs.SetInt("LastEquippedCharacterIndex", characterIndex); // En son se�ilen karakterin index'ini kaydet
        selectButton.gameObject.SetActive(false); // Se� butonunu gizle
        UpdateCatalogUI(characterIndex); // Katalogda tick i�aretini g�ncelle
    }

    // Katalogdaki Tick ��aretini G�ncelleme Fonksiyonu
    public void UpdateCatalogUI(int selectedCharacterIndex)
    {
        // T�m butonlardaki tick i�aretlerini gizle
        for (int i = 0; i < catalogCharacterButtons.Length; i++)
        {
            Transform tickTransform = catalogCharacterButtons[i].transform.Find("Tick");

            if (tickTransform != null)
            {
                tickTransform.gameObject.SetActive(false); // Tick i�aretini gizle
            }
            else
            {
                Debug.LogWarning("Tick object not found on button " + i);
            }
        }

        // Se�ilen karakterin butonuna tick i�aretini g�ster
        Transform selectedTickTransform = catalogCharacterButtons[selectedCharacterIndex].transform.Find("Tick");

        if (selectedTickTransform != null)
        {
            selectedTickTransform.gameObject.SetActive(true); // Se�ilen karakterin tick i�aretini g�ster
        }
        else
        {
            Debug.LogWarning("Tick object not found on selected button");
        }
    }

    // �lk Karakteri �cretsiz Sunma Fonksiyonu
    public void OfferFirstCharacterForFree()
    {
        // Hi�bir karakter sat�n al�nmam��sa
        bool anyCharacterPurchased = false;

        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            if (PlayerPrefs.GetInt("CharacterPurchased_" + i) == 1)
            {
                anyCharacterPurchased = true;
                break;
            }
        }

        // E�er hi�bir karakter sat�n al�nmam��sa
        if (!anyCharacterPurchased)
        {
            PlayerPrefs.SetInt("CharacterPurchased_0", 1); // �lk karakteri �cretsiz olarak sat�n al�nm�� gibi i�aretle
            PlayerPrefs.SetInt("SelectedCharacter", 0); // �lk karakteri se�ili olarak ayarla
            PlayerPrefs.SetInt("LastEquippedCharacter", 0); // �lk karakteri son kullan�lan karakter olarak ayarla
            PlayerPrefs.SetInt("defaultCharacter", 0); // �lk karakteri varsay�lan karakter olarak ayarla
            UpdateCatalogUI(0); // Katalogdaki tick i�aretini g�ncelle
        }
    }

    // Oyun ba�larken �al��t�r�lacak fonksiyon
    
}
