using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    private string saveLocation;
    private string questSaveLocation;
    private string questHandinLocation;
    private string shopSaveLocation;         // File JSON riêng cho shop
    private InventoryController inventoryController;
    private HotBarController hotBarController;
    private Chest[] chests;
    [SerializeField] private GameObject StatusImage;
    private ShopNPC[] shopNPCs;
    public static bool isNewGame = false;
    
    void Start()
    {

        InitializeComponent();
        LoadGame();
    }
    private void InitializeComponent()
    {
        //define save location
        saveLocation     = Path.Combine(Application.persistentDataPath, "saveData.json");
        questSaveLocation = Path.Combine(Application.persistentDataPath, "questProgress.json");
        shopSaveLocation  = Path.Combine(Application.persistentDataPath, "shopData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotBarController    = FindObjectOfType<HotBarController>();
        chests   = FindObjectsOfType<Chest>();
        shopNPCs = FindObjectsOfType<ShopNPC>();
        //thông báo lưu thành công
        StatusImage.SetActive(false);
    }

    public void saveGame()
    {
        // Prepare main save data (remove quest data from main file to avoid duplication)
        SaveData saveData = new SaveData
        {
            PlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            MapBoundary = FindObjectOfType<CinemachineConfiner2D>().m_BoundingShape2D.gameObject.name,
            InvetorySaveData = inventoryController.GetInventoryItems(),
            HotBarSaveData = hotBarController.GetBarItems(),
            chestsSaveData = GetChestsState(),
            HandleIDs = QuestController.instance.handinQuestIDs,
            Gold = CurrencyController.instance.GetGold(),
            CurrentAmmo = AmmoManager.Instance != null ? AmmoManager.Instance.GetCurrentAmmo() : 30,
            MaxAmmo = AmmoManager.Instance != null ? AmmoManager.Instance.GetMaxAmmo() : 99,
            shopStates = null,            // đã chuyển sang file riêng shopData.json
            questProgressesData = null,   // intentionally null — quests will be saved to separate file
            gameStats = GameStats.Instance != null ? GameStats.Instance.ToSaveData() : null,
            VolumeBGM = SoundManager.Instance != null ? SoundManager.Instance.GetVolumeBGM() : 1f,
            VolumeSFX = SoundManager.Instance != null ? SoundManager.Instance.GetVolumeSFX() : 1f
        };

        // Ghi file save chính
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));

        // Ghi shop ra file JSON riêng
        SaveShopsToFile();

        // Ghi file save chính (cũ đã được ghi ở trên)
        Debug.Log(saveLocation);

        // Ghi quest progress ra file riêng
        if (QuestController.instance != null)
        {
            var wrapper = new QuestProgressSaveWrapper
            {
                questProgresses = QuestController.instance.activeQuests ?? new List<QuestProgress>()
            };
            File.WriteAllText(questSaveLocation, JsonUtility.ToJson(wrapper));
            Debug.Log("Quest save written to: " + questSaveLocation);
        }

        StartCoroutine(ShowMessage());

        IEnumerator ShowMessage()
        {
            StatusImage.SetActive(true);
            yield return new WaitForSeconds(2);
            StatusImage.SetActive(false);
        }
    }

    /// <summary>
    /// Gọi khi game sắp chuyển sang EndScene.
    /// Lưu toàn bộ dữ liệu và đánh dấu gameCompleted = true.
    /// </summary>
    public void SaveBeforeEnd()
    {
        // Lưu thông thường trước
        saveGame();

        // Cập nhật thêm flag gameCompleted vào file
        if (File.Exists(saveLocation))
        {
            SaveData existing = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            existing.gameCompleted = true;
            File.WriteAllText(saveLocation, JsonUtility.ToJson(existing));
            Debug.Log("[GameController] ✅ Đã lưu game + đánh dấu gameCompleted = true.");
        }
    }

    // ─── Shop: Lưu và load riêng ra shopData.json ─────────────────────────────
    private void SaveShopsToFile()
    {
        var wrapper = new ShopSaveWrapper { shops = GetShopStates() };
        File.WriteAllText(shopSaveLocation, JsonUtility.ToJson(wrapper));
        Debug.Log("[GameController] Shop đã lưu ra: " + shopSaveLocation);
    }

    private void LoadShopsFromFile()
    {
        if (!File.Exists(shopSaveLocation))
        {
            Debug.Log("[GameController] Chưa có file shopData.json, dùng stock mặc định.");
            return;
        }
        var wrapper = JsonUtility.FromJson<ShopSaveWrapper>(File.ReadAllText(shopSaveLocation));
        LoadShopState(wrapper?.shops);
    }

    private List<ShopIntanceData> GetShopStates()
    {
        List<ShopIntanceData> shopStates = new List<ShopIntanceData>();
        foreach (ShopNPC shop in shopNPCs)
        {
            ShopIntanceData shopData = new ShopIntanceData
            {
                shopID = shop.ShopID,
                stock = new List<ShopItemData>()
            };
            foreach(ShopNPC.ShopstockItem item in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData
                {
                    itemID = item.itemID,
                    quantity = item.quantity
                });
            }
            shopStates.Add(shopData);
        }
        return shopStates;
    }
    private List<ChestsSaveData> GetChestsState()
    {
        List<ChestsSaveData> chestSate = new List<ChestsSaveData>();
        foreach (Chest chest in chests)
        {
            ChestsSaveData chestsSaveData = new ChestsSaveData
            {
                ChestID = chest.ChestID,
                isOpened = chest.IsOpened,
            };
            chestSate.Add(chestsSaveData);
        }
        return chestSate;
    }
    public void LoadGame()
    {
        if (isNewGame)
        {
            Debug.Log("New Game - không load save");

            isNewGame = false;

            inventoryController.SetInventoryItems(new List<InvetorySaveData>());
            hotBarController.SetHotBarItems(new List<InvetorySaveData>());
            MapController.Instance?.GenerateMap();

            // Reset đạn về mặc định khi new game
            if (AmmoManager.Instance != null)
                AmmoManager.Instance.ResetAmmo();

            // Reset thống kê khi bắt đầu game mới
            GameStats.Instance?.Reset();

            return;
        }
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.PlayerPosition;

            PolygonCollider2D saveMapBoundry = GameObject.Find(saveData.MapBoundary).GetComponent<PolygonCollider2D>();
            FindObjectOfType<CinemachineConfiner2D>().m_BoundingShape2D = saveMapBoundry;

            MapController.Instance?.GenerateMap(saveMapBoundry);
            inventoryController.SetInventoryItems(saveData.InvetorySaveData);
            hotBarController.SetHotBarItems(saveData.HotBarSaveData);
            QuestController.instance.handinQuestIDs = saveData.HandleIDs;

            //Load chests state
            LoadChestState(saveData.chestsSaveData);
            LoadShopState(saveData.shopStates);
            CurrencyController.instance.SetGold(saveData.Gold);

            // Load số đạn (chỉ load nếu save có dữ liệu đạn hợp lệ, tránh save cũ ghi đè = 0)
            if (AmmoManager.Instance != null && saveData.MaxAmmo > 0)
                AmmoManager.Instance.SetAmmo(saveData.CurrentAmmo, saveData.MaxAmmo);


            // Load quest progress: prefer separate quest file; fallback to quest data embedded in main save (for backward compat)
            if (File.Exists(questSaveLocation))
            {
                var wrapper = JsonUtility.FromJson<QuestProgressSaveWrapper>(File.ReadAllText(questSaveLocation));
                QuestController.instance.LoadQuestProgress(wrapper?.questProgresses ?? new List<QuestProgress>());
            }
            else
            {
                // backward compatibility: if old save had quests inside main file
                QuestController.instance.LoadQuestProgress(saveData.questProgressesData);
            }

            // Load thống kê gameplay nếu có trong file save
            if (saveData.gameStats != null)
                GameStats.Instance?.LoadFromSave(saveData.gameStats);

            // Áp dụng lại cài đặt âm lượng đã lưu
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMasterVolumeBGM(saveData.VolumeBGM > 0 ? saveData.VolumeBGM : 1f);
                SoundManager.Instance.SetVolumeSFX(saveData.VolumeSFX > 0 ? saveData.VolumeSFX : 1f);
            }

            // Load shop từ file riêng
            LoadShopsFromFile();
        }
        else
        {
            if (isNewGame==false)
            {
                saveGame();
                inventoryController.SetInventoryItems(new List<InvetorySaveData>());
                hotBarController.SetHotBarItems(new List<InvetorySaveData>());
                MapController.Instance?.GenerateMap();
            }
                
        }
    }

    public void LoadChestState(List<ChestsSaveData> chestsState)
    {
        foreach (Chest chest in chests)
        {
            Debug.Log("Chest ID: " + chest.ChestID);
            ChestsSaveData chestSaveData = chestsState.FirstOrDefault(c => c.ChestID == chest.ChestID);
            if (chestSaveData != null)
            {
                chest.SetOpend(chestSaveData.isOpened);
            }
        }
    }
    void LoadShopState(List<ShopIntanceData> shopStates)
    {
        if (shopStates == null)
        {
            Debug.LogWarning("[GameController] Không có dữ liệu shop để load.");
            return;
        }
        foreach (ShopNPC shop in shopNPCs)
        {
            ShopIntanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.ShopID);
            if (shopData != null)
            {
                // REPLACE toàn bộ (không Add để tránh nhân đôi stock mỗi lần load)
                List<ShopNPC.ShopstockItem> loadedStock = new List<ShopNPC.ShopstockItem>();
                foreach (ShopItemData itemData in shopData.stock)
                {
                    loadedStock.Add(new ShopNPC.ShopstockItem
                    {
                        itemID = itemData.itemID,
                        quantity = itemData.quantity
                    });
                }
                shop.SetStock(loadedStock);
                Debug.Log($"[GameController] Load shop '{shop.ShopID}': {loadedStock.Count} loại hàng.");
            }
        }
    }
    public void ClearAllData()
    {
        isNewGame = true;
        string path1 = Application.persistentDataPath + "/saveData.json";
        string path2 = Application.persistentDataPath + "/questProgress.json";
        string path3 = Application.persistentDataPath + "/shopData.json";

        if (File.Exists(path1)) File.Delete(path1);
        if (File.Exists(path2)) File.Delete(path2);
        if (File.Exists(path3)) File.Delete(path3);
    }
}