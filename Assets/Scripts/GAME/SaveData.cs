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

}
[System.Serializable]
public class ChestsSaveData 
{
    public string ChestID;
    public bool isOpened;
}

