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

}
[System.Serializable]
public class ChestsSaveData 
{
    public string ChestID;
    public bool isOpened;
}

// Wrapper để JsonUtility có thể serialize/deserialize top-level List<QuestProgress>
[System.Serializable]
public class QuestProgressSaveWrapper
{
    public List<QuestProgress> questProgresses;
}
