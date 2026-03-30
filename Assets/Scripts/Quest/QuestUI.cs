using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questContent;
    public GameObject questEntryPrefab;
    public GameObject objectTitlePrefab;

 
    void Start()
    {
       
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        if (questContent == null)
        {
            Debug.LogError("QuestUI: questContent is not assigned.");
            return;
        }

        if (questEntryPrefab == null)
        {
            Debug.LogError("QuestUI: questEntryPrefab is not assigned.");
            return;
        }

        if (objectTitlePrefab == null)
        {
            Debug.LogError("QuestUI: objectTitlePrefab is not assigned.");
            return;
        }

        if (QuestController.instance == null)
        {
            Debug.LogWarning("QuestUI: QuestController.instance is null. No quests to display.");
            return;
        }

        // Destroy existing quest entry
        foreach (Transform child in questContent)
        {
            Destroy(child.gameObject);
        }

        // Build quest content
        foreach (var questContentNew in QuestController.instance.activeQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questContent);
            if (entry == null)
            {
                Debug.LogWarning("QuestUI: Failed to instantiate questEntryPrefab.");
                continue;
            }

            var questNameTransform = entry.transform.Find("QuestNameText");
            TMP_Text questNametext = questNameTransform != null ? questNameTransform.GetComponent<TMP_Text>() : null;
            if (questNametext == null)
            {
                Debug.LogWarning("QuestUI: quest entry prefab is missing 'QuestNameText' TMP_Text. Skipping entry.");
                Destroy(entry);
                continue;
            }

            Transform objectiveList = entry.transform.Find("ObjectiveList");
            if (objectiveList == null)
            {
                Debug.LogWarning("QuestUI: quest entry prefab is missing 'ObjectiveList' transform. Skipping entry.");
                Destroy(entry);
                continue;
            }

            questNametext.text = questContentNew.quest != null ? questContentNew.quest.name : "Unknown Quest";

            foreach (var objective in questContentNew.objectives)
            {
                GameObject objectiveTextGo = Instantiate(objectTitlePrefab, objectiveList);
                if (objectiveTextGo == null)
                {
                    Debug.LogWarning("QuestUI: Failed to instantiate objectTitlePrefab.");
                    continue;
                }

                TMP_Text objecttext = objectiveTextGo.GetComponent<TMP_Text>();
                if (objecttext == null)
                {
                    Debug.LogWarning("QuestUI: objectTitlePrefab does not contain a TMP_Text component. Destroying created object.");
                    Destroy(objectiveTextGo);
                    continue;
                }

                objecttext.text = $"{objective.description} ({objective.currentSellect}/{objective.requiredSellect})";
            }
        }
    }
}
