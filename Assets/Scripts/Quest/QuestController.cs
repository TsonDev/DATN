using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController instance { get; private set; }
    public List<QuestProgress> activeQuests = new List<QuestProgress>();
    private QuestUI questUI;

    public List<string> handinQuestIDs = new();

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
        questUI = FindAnyObjectByType<QuestUI>();
        InventoryController.Intance.OnInventoryChanged += CheckInventoryForQuests;
    }
    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;
        activeQuests.Add(new QuestProgress(quest));
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }
    public bool IsQuestActive(string questID)
    {
        return activeQuests.Exists(q => q != null && q.quest != null && q.QuestID == questID);
    }
    public void CheckInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = InventoryController.Intance.GetItemCounts();
        foreach (QuestProgress quest in activeQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if(questObjective.type != QuestObjectType.CollectItem) continue;
                //if(!int.TryParse(questObjective.objectiveID, out int itemID)) continue;
                int newAmount = itemCounts.TryGetValue(questObjective.objectiveID, out int count) ? Mathf.Min(count, questObjective.requiredSellect) : 0;
                if (questObjective.currentSellect != newAmount)
                {
                    questObjective.currentSellect = newAmount;
                }
            }
        }
        questUI.UpdateQuestUI();
    }
    public bool IsQuestComplete(string questID)
    {
        QuestProgress questProgress = activeQuests.Find(q=>q.QuestID == questID);
        return questProgress != null && questProgress.objectives.TrueForAll(o => o.IsCompleted);
    }
    public void HandInQuest(string questID)
    {
        Debug.Log($"HandInQuest requested for {questID}");
        // remove item required
        if (!RemoveItemRequired(questID))
        {
            Debug.LogWarning($"HandInQuest: failed to remove required items for quest {questID}");
            return;
        }
        // remove quest in quest panel log
        QuestProgress quest = activeQuests.Find(q => q.QuestID.Equals(questID));
        if (quest != null)
        {
            handinQuestIDs.Add(questID);
            activeQuests.Remove(quest);
            Debug.Log($"HandInQuest: quest {questID} removed from activeQuests.");
            questUI?.UpdateQuestUI();
        }
    }
    public bool IsHandin(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }
    public bool RemoveItemRequired(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q != null && q.QuestID == questID);
        if (quest == null)
        {
            Debug.LogWarning($"RemoveItemRequired: quest {questID} not found.");
            return false;
        }

        // aggregate required items (sum if multiple objectives use same item)
        Dictionary<int, int> required = new Dictionary<int, int>();
        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective == null) continue;
            if (objective.type == QuestObjectType.CollectItem)
            {
                required[objective.objectiveID] = required.GetValueOrDefault(objective.objectiveID, 0) + objective.requiredSellect;
            }
        }

        if (required.Count == 0)
        {
            Debug.Log($"RemoveItemRequired: quest {questID} has no collect objectives.");
            return true;
        }

        if (InventoryController.Intance == null)
        {
            Debug.LogWarning("RemoveItemRequired: InventoryController.Intance is null.");
            return false;
        }

        Dictionary<int, int> itemCounts = InventoryController.Intance.GetItemCounts();

        // debug output required vs have
        foreach (var kv in required)
        {
            Debug.Log($"RemoveItemRequired: need itemID {kv.Key} x{kv.Value}; have {itemCounts.GetValueOrDefault(kv.Key,0)}");
        }

        // verify
        foreach (var kv in required)
        {
            if (!itemCounts.TryGetValue(kv.Key, out int have) || have < kv.Value)
            {
                Debug.LogWarning($"RemoveItemRequired: not enough of item {kv.Key} (have {have}, need {kv.Value})");
                return false;
            }
        }

        // remove items
        foreach (var kv in required)
        {
            Debug.Log($"RemoveItemRequired: removing itemID {kv.Key} x{kv.Value}");
            InventoryController.Intance.RemoveItemFromInventory(kv.Key, kv.Value);
        }

        return true;
    }
    public void LoadQuestProgress(List<QuestProgress> saveQuests)
    {
        activeQuests = saveQuests ?? new();
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();

    }
}
