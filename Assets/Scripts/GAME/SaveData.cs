using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 PlayerPosition;
    public string MapBoundary;//name map
    public List< InvetorySaveData> InvetorySaveData;
    public List< InvetorySaveData> HotBarSaveData;
    public List<ChestsSaveData> chestsSaveData;
    public List<QuestProgress> questProgressesData;
    public List<string> HandleIDs;
    public int Gold;
    public int CurrentAmmo;
    public int MaxAmmo;
    public List<ShopIntanceData> shopStates = new List<ShopIntanceData>();

}
[System.Serializable]
public class ChestsSaveData 
{
    public string ChestID;
    public bool isOpened;
}
[System.Serializable]
public class ShopIntanceData
{
    public string shopID;
    public List<ShopItemData> stock = new List<ShopItemData>();
}
[System.Serializable]
public class ShopItemData
{
    public int itemID;
    public int quantity;
}
// Wrapper để JsonUtility có thể serialize/deserialize top-level List<QuestProgress>
[System.Serializable]
public class QuestProgressSaveWrapper
{
    public List<QuestProgress> questProgresses;
}
