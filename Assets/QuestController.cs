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
    }
    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;
        activeQuests.Add(new QuestProgress(quest));
        questUI.UpdateQuestUI();
    }
    public bool IsQuestActive(string questID)=>activeQuests.Exists(q=>q.QuestID == questID);
}
