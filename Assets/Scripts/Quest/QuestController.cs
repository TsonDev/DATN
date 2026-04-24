using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                // --- CollectItem: cập nhật progress thông thường ---
                if (questObjective.type == QuestObjectType.CollectItem)
                {
                    int newAmount = itemCounts.TryGetValue(questObjective.objectiveID, out int count)
                        ? Mathf.Min(count, questObjective.requiredSellect) : 0;
                    if (questObjective.currentSellect != newAmount)
                        questObjective.currentSellect = newAmount;
                }

                // --- CheckItemLoadScene: kiểm tra số lượng item, nếu đủ thì load scene ---
                if (questObjective.type == QuestObjectType.CheckItemLoadScene)
                {
                    int have = itemCounts.TryGetValue(questObjective.objectiveID, out int c) ? c : 0;
                    questObjective.currentSellect = Mathf.Min(have, questObjective.requiredSellect);

                    if (questObjective.IsCompleted && !string.IsNullOrEmpty(questObjective.sceneToLoad))
                    {
                        Debug.Log($"[Quest] ✅ CheckItemLoadScene đủ điều kiện. Load scene: {questObjective.sceneToLoad}");
                        SceneManager.LoadScene(questObjective.sceneToLoad);
                        return; // dừng vòng lặp vì scene đang được load
                    }
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

    // ─────────────────────────────────────────────────────────────────────────
    // Báo cáo progress cho các loại objective không phải CollectItem
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi khi giết quái. enemyID phải khớp với objectiveID trong Quest.
    /// </summary>
    public void ReportEnemyKilled(int enemyID)
    {
        AddProgress(QuestObjectType.DefeatEnemy, enemyID, 1);
    }

    /// <summary>
    /// Gọi khi kết thúc hội thoại với NPC. npcID phải khớp với objectiveID trong Quest.
    /// </summary>
    public void ReportNpcTalked(int npcID)
    {
        AddProgress(QuestObjectType.TalkNpc, npcID, 1);
    }

    /// <summary>
    /// Gọi thủ công từ bất kỳ script nào (trigger zone, event, cutscene...).
    /// objectiveID phải khớp với objectiveID được gán trong Quest ScriptableObject.
    /// </summary>
    public void ReportCustomProgress(int objectiveID, int amount = 1)
    {
        AddProgress(QuestObjectType.Custom, objectiveID, amount);
    }

    /// <summary>
    /// Logic nội bộ: tìm tất cả active quests có objective khớp type + ID,
    /// cộng progress và cập nhật UI.
    /// </summary>
    private void AddProgress(QuestObjectType type, int objectiveID, int amount)
    {
        bool changed = false;
        foreach (QuestProgress quest in activeQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type != type) continue;
                if (objective.objectiveID != objectiveID) continue;
                if (objective.IsCompleted) continue;

                objective.currentSellect = Mathf.Min(
                    objective.currentSellect + amount,
                    objective.requiredSellect
                );
                changed = true;
                Debug.Log($"[Quest] ✅ {type} ID={objectiveID} → {objective.currentSellect}/{objective.requiredSellect}");
            }
        }
        if (changed)
        {
            questUI?.UpdateQuestUI();
        }
        else
        {
            // Không tìm thấy objective khớp: có thể quest chưa nhận hoặc ID bị sai
            if (activeQuests.Count > 0)
                Debug.LogWarning($"[Quest] ⚠️ AddProgress không tìm thấy objective type={type} ID={objectiveID}. " +
                    $"Kiểm tra: (1) Quest đã được nhận chưa? (2) objectiveID trong Quest SO có khớp với ID trên NPC/Enemy không?");
        }
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
