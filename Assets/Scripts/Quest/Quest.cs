using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string questDescription;
    public List<QuestObjective> objectives;
    public List<QuestReward> questRewards;

    //Called when have Scripttable obj created
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
    
}
[System.Serializable]
public class QuestObjective
{
    public int objectiveID;//MATCH with itemID collect, enemyID kill
    public string description;
    public int currentSellect;
    public int requiredSellect;

    public bool IsCompleted => currentSellect >= requiredSellect;
    public QuestObjectType type;
}
public enum QuestObjectType { CollectItem, DefeatEnemy, ReachLocation, TalkNpc, Custom }

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;
    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();
        //Tạo copy
        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                currentSellect = 0,
                requiredSellect = obj.requiredSellect,
                type = obj.type,
            });
        }
    }
    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.questID;
}
[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public int rewardID;
    public int amount =1;
}
public enum RewardType { Experience, Item, Both, Gold, Custom }
