using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController instance { get; private set; }
    public List<QuestProgress> activeQuests = new List<QuestProgress>();
    private QuestUI questUI;

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
    public bool IsQuestActive(string questID)=>activeQuests.Exists(q=>q.QuestID == questID);
    public void CheckInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = InventoryController.Intance.GetItemCounts();
        foreach (QuestProgress quest in activeQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if(questObjective.type != QuestObjectType.CollectItem) continue;
                if(!int.TryParse(questObjective.objectiveID, out int itemID)) continue;
                int newAmount = itemCounts.TryGetValue(itemID, out int count) ? Mathf.Min(count, questObjective.requiredSellect) : 0;
                if (questObjective.currentSellect != newAmount)
                {
                    questObjective.currentSellect = newAmount;
                }
            }
        }
        questUI.UpdateQuestUI();
    }
    public void LoadQuestProgress(List<QuestProgress> saveQuests)
    {
        activeQuests = saveQuests ?? new();
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();

    }
}
