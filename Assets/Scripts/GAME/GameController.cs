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
    private InventoryController inventoryController;
    private HotBarController hotBarController;
    private Chest[] chests;
    [SerializeField] private GameObject StatusImage;
    void Start()
    {

        InitializeComponent();
        LoadGame();
    }
    private void InitializeComponent()
    {
        //define save locayion
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotBarController = FindObjectOfType<HotBarController>();
        chests = FindObjectsOfType<Chest>();
        //thông báo lưu thành công
        StatusImage.SetActive(false);
    }

    public void saveGame()
    {
        SaveData saveData = new SaveData
        {
            PlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            MapBoundary = FindObjectOfType<CinemachineConfiner2D>().m_BoundingShape2D.gameObject.name,
            InvetorySaveData = inventoryController.GetInventoryItems(),
            HotBarSaveData = hotBarController.GetBarItems(),
            chestsSaveData = GetChestsState(),
            questProgressesData = QuestController.instance.activeQuests

        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log(saveLocation);
        StartCoroutine(ShowMessage());

        IEnumerator ShowMessage()
        {
            StatusImage.SetActive(true);
            yield return new WaitForSeconds(2);
            StatusImage.SetActive(false);
        }
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
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.PlayerPosition;

            PolygonCollider2D saveMapBoundry = GameObject.Find(saveData.MapBoundary).GetComponent<PolygonCollider2D>();
            FindObjectOfType<CinemachineConfiner2D>().m_BoundingShape2D = saveMapBoundry;

            MapController.Instance?.GenerateMap(saveMapBoundry);
            inventoryController.SetInventoryItems(saveData.InvetorySaveData);
            hotBarController.SetHotBarItems(saveData.HotBarSaveData);

            //Load chests state
            LoadChestState(saveData.chestsSaveData);
            QuestController.instance.LoadQuestProgress(saveData.questProgressesData);

        }
        else
        {
            saveGame();
            inventoryController.SetInventoryItems(new List<InvetorySaveData>());
            hotBarController.SetHotBarItems(new List<InvetorySaveData>());
            MapController.Instance?.GenerateMap();
        }
    }

    public void LoadChestState(List<ChestsSaveData> chestsState)
    {
        foreach(Chest chest in chests)
        {
            Debug.Log("Chest ID: " + chest.ChestID);
            ChestsSaveData chestSaveData = chestsState.FirstOrDefault(c=>c.ChestID== chest.ChestID);
            if (chestSaveData != null)
            {
                chest.SetOpend(chestSaveData.isOpened);
            }
        }
    }
   
}
