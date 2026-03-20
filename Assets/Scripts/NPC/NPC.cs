using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour,IInteractable
{
    public NPCDialog dialogData;
    [SerializeField] SoundData talkSound;

    private int dialogIndex;
    private bool isTyping, isDialogActive;
    private enum QuestState { Notstarted, InProgress, Completed };
    private QuestState currentQuestState = QuestState.Notstarted;   
    public bool CanInteract()
    {
        return !isDialogActive;
    }
    public void Interact()
    {
        if(dialogData == null ) /*pauseController &&*//*!isDialogActive)*/
        {
            return;
        }
        if (isDialogActive)
        {
            //Next line
            NextLine();
        }
        else
        {
            //Start dialog
            StartDialog();
        }
    }
    void StartDialog()
    {
        //Sysc with quest data
        SyncQuestState();
        //Set dialog data based on quest state
        if (currentQuestState == QuestState.Notstarted)
        {
            dialogIndex = 0;
        }

        isDialogActive = true;
        dialogIndex = 0;
        //nameText.SetText(dialogData.npcName);
        //portraitImage.sprite = dialogData.npcPortrait;
        //npcDialogPanel.SetActive(true);

        DialogController.instance.SetNpcInfor(dialogData.npcName, dialogData.npcPortrait);
        DialogController.instance.ShowDialogUI(true);
        //pause game;
        Time.timeScale = 0f;
        //StartCoroutine(Typeline());
        DisplayCurrentLine();
    }
    private void SyncQuestState()
    {
        if (dialogData.quest == null) return;
        string questID = dialogData.quest.questID;
        //Future update add completing quest and handing in
        if (QuestController.instance.IsQuestActive(questID))
        {
            currentQuestState = QuestState.InProgress;
        }
        else
        {
            currentQuestState = QuestState.Completed;
        }
    }
    void NextLine()
    {
        if (isTyping)
        {
            //skip animation and show all dialog
            StopAllCoroutines();
            //dialogText.SetText(dialogData.dialogLines[dialogIndex]);
            DialogController.instance.SetDialogText(dialogData.dialogLines[dialogIndex]);
            isTyping = false;
        }
        //Clear choices
        DialogController.instance.ClearCHoices();
        //Check endprogressLines
        if(dialogData.EndProgressLines.Length > dialogIndex && dialogData.EndProgressLines[dialogIndex])
        {
            EndDialog();
            return;
        }
        //Check if choice & display
        foreach (DialogChoice dialogChoice in dialogData.choices)
        {
            if(dialogChoice.dialogIndex == dialogIndex)
            {
                //Display choice
                DisplayChoices(dialogChoice);
                return;
            }
        }


        if (++dialogIndex < dialogData.dialogLines.Length)
        {
            //if another line, type next line
            DisplayCurrentLine();
            //StartCoroutine(Typeline());
        }
        else
        {
            //EndDialog
            EndDialog();
        }
    }
    IEnumerator Typeline()
    {
        isTyping = true;
        //dialogText.SetText("");
        DialogController.instance.SetDialogText("");
        int counter = 0;
        foreach (char letter in dialogData.dialogLines[dialogIndex])
        {
            //dialogText.text += letter;
            DialogController.instance.dialogText.text += letter;
            //yield return new WaitForSeconds(dialogData.typingSpeed);
            counter++;
            if (counter % 3 == 0)
            {
                SoundManager.Instance.PlaySound(talkSound);
            }
            
            yield return new WaitForSecondsRealtime(dialogData.typingSpeed);
        }
        isTyping = false;
        if(dialogData.autoProgressLines.Length > dialogIndex && dialogData.autoProgressLines[dialogIndex])
        {
            yield return new WaitForSeconds(dialogData.autoProgressDisplay);
            //Display nextline
        }
    }

    void DisplayChoices(DialogChoice dialogChoice)
    {
        for (int i = 0; i < dialogChoice.choices.Length; i++)
        {
            int indexChoice = dialogChoice.nextDialogIndexes[i];
            DialogController.instance.CreateButtonChoice(dialogChoice.choices[i], ()=>ChoiceOption(indexChoice));
        }
    }
    void ChoiceOption(int nextindex)
    {
        dialogIndex = nextindex;
        DialogController.instance.ClearCHoices();
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(Typeline());
    }
    public void EndDialog()
    {
        StopAllCoroutines();
        isDialogActive = false;
        //dialogText.SetText("");
        //npcDialogPanel.SetActive(false);
        DialogController.instance.SetDialogText("");
        DialogController.instance.ShowDialogUI(false);
        Time.timeScale = 1f;
        //pauseController.setActive(false);
    }
}
