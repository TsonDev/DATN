using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questContent;
    public GameObject questEntryPrefab;
    public GameObject objectTitlePrefab;

    //public Quest testQuest;
    //public int testQuestAmount;
    //private List<QuestProgress> testQuestList = new();
    // Start is called before the first frame update
    void Start()
    {
        //for(int i = 0; i<testQuestAmount; i++)
        //{
        //    testQuestList.Add(new QuestProgress(testQuest));
        //}
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        //Destroy existing quest entry
        foreach(Transform child in questContent)
        {
            Destroy(child.gameObject);
            
        }
        //Build quest content
        foreach(var questContentNew in QuestController.instance.activeQuests )
        {
            GameObject entry = Instantiate(questEntryPrefab, questContent);
            TMP_Text questNametext = entry.transform.Find("QuestNameText").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNametext.text = questContentNew.quest.name;
            foreach(var objective in questContentNew.objectives)
            {
                GameObject objectiveTextGo = Instantiate(objectTitlePrefab,objectiveList);
                TMP_Text objecttext = objectiveTextGo.GetComponent<TMP_Text>();
                objecttext.text = $"{objective.description} ({objective.currentSellect}/{objective.requiredSellect})";
            }
        }
    }
}
