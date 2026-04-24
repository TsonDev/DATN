using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour,IInteractable
{
    [Tooltip("Hội thoại/Nhiệm vụ đơn (chỉ chạy 1 lần)")]
    public NPCDialog dialogData;
    
    [Tooltip("Chuỗi Hội thoại/Nhiệm vụ. Chạy theo thứ tự từ trên xuống dưới cứ mỗi khi trả xong một nhiệm vụ.")]
    public NPCDialog[] dialogChain;

    [SerializeField] SoundData talkSound;

    [Header("Quest")]
    [Tooltip("ID này phải khớp với objectiveID trong Quest loại TalkNpc. Để 0 nếu NPC này không thuộc quest nào.")]
    public int npcID = 0;

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
        if(dialogData == null )
        {
            Debug.LogWarning($"[NPC] '{gameObject.name}' chưa có DialogData! Hãy tạo NPCDialog ScriptableObject và gán vào field 'Dialog Data' trong Inspector.");
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
        else if(currentQuestState == QuestState.InProgress)
        {
            dialogIndex = dialogData.questInprogressIndex;
        }
        else if(currentQuestState == QuestState.Completed)
        {
            dialogIndex = dialogData.questCompletedIndex;
        }

        // --- Kiểm tra an toàn chống lỗi (Safety Check) ---
        if (dialogData == null || dialogData.dialogLines == null || dialogData.dialogLines.Length == 0)
        {
            Debug.LogWarning("Hội thoại NPC của bạn chưa có chữ nào ở Dialog Lines cả! Bỏ qua hội thoại.");
            return;
        }
        if (dialogIndex >= dialogData.dialogLines.Length || dialogIndex < 0)
        {
            Debug.LogWarning("Chỉ số Quest Index lớn hơn số dòng hội thoại thực tế! Hãy kiểm tra lại questInprogressIndex hoặc questCompletedIndex trong Inspector. Tự động lùi về vị trí an toàn.");
            dialogIndex = dialogData.dialogLines.Length - 1;
        }

        isDialogActive = true;
       
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
    private void UpdateDialogData()
    {
        if (dialogChain != null && dialogChain.Length > 0)
        {
            for (int i = 0; i < dialogChain.Length; i++)
            {
                dialogData = dialogChain[i];
                
                // Nếu NPC ở mốc này có Quest và Quest đó ĐÃ ĐƯỢC TRẢ THƯỞNG xong
                if (dialogData.quest != null && QuestController.instance.IsHandin(dialogData.quest.questID))
                {
                    // Nếu không phải là đoạn hội thoại cuối cùng trong mảng, thì nhảy sang đoạn tiếp theo
                    if (i < dialogChain.Length - 1)
                    {
                        continue;
                    }
                }
                else
                {
                    // Nếu chưa trả thưởng xong (có thể là chưa nhận, hoặc đang làm), thì chốt dừng ở hội thoại này
                    break;
                }
            }
        }
    }

    private void SyncQuestState()
    {
        UpdateDialogData();

        if (dialogData == null || dialogData.quest == null) 
        {
            currentQuestState = QuestState.Notstarted;
            return;
        }
        string questID = dialogData.quest.questID;
        //Future update add completing quest and handing in
        if (QuestController.instance.IsQuestComplete(questID) || QuestController.instance.IsHandin (questID))
        {
            currentQuestState = QuestState.Completed;
        }
        else if (QuestController.instance.IsQuestActive(questID))
        {
            currentQuestState = QuestState.InProgress;
        }
        else
        {
            currentQuestState = QuestState.Notstarted;
        }
    }
    void NextLine()
    {
        if (isTyping)
        {
            //skip animation and show all dialog
            StopAllCoroutines();
            DialogController.instance.SetDialogText(dialogData.dialogLines[dialogIndex]);
            isTyping = false;
            return; // Chỉ skip animation, không chạy tiếp — chờ bấm lần nữa
        }
        //Clear choices
        DialogController.instance.ClearCHoices();
        //Check endprogressLines
        if(dialogData.EndProgressLines.Length > dialogIndex && dialogData.EndProgressLines[dialogIndex])
        {
            Debug.Log($"[NPC] EndProgressLines[{dialogIndex}] = true → EndDialog");
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
            Debug.Log($"[NPC] Chuyển sang dòng {dialogIndex}: \"{dialogData.dialogLines[dialogIndex]}\"");
            DisplayCurrentLine();
        }
        else
        {
            Debug.Log($"[NPC] Hết dòng hội thoại ({dialogIndex}) → EndDialog");
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
            bool giveQuest = dialogChoice.giveQuest[i];
            DialogController.instance.CreateButtonChoice(dialogChoice.choices[i], ()=>ChoiceOption(indexChoice,giveQuest));
        }
    }
    void ChoiceOption(int nextindex, bool giveQues)
    {
        if(giveQues)
        {
            QuestController.instance.AcceptQuest(dialogData.quest);
            currentQuestState = QuestState.InProgress;
        }
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
        DialogController.instance.SetDialogText("");
        DialogController.instance.ShowDialogUI(false);
        Time.timeScale = 1f;

        // Báo cáo TalkNpc TRƯỚC — để SyncQuestState lần sau đọc được progress đúng
        if (npcID != 0)
        {
            QuestController.instance?.ReportNpcTalked(npcID);
        }

        // Kiểm tra hoàn thành quest của NPC này (null-safe)
        if (currentQuestState == QuestState.Completed
            && dialogData?.quest != null
            && !QuestController.instance.IsHandin(dialogData.quest.questID))
        {
            HandleQuestComplete(dialogData.quest);
        }
    }

    public void HandleQuestComplete(Quest quest)
    {
        RewardController.instance.GiveQuestReward(quest);
        QuestController.instance.HandInQuest(quest.questID);
        
        // Ngay sau khi trả nhiệm vụ, cập nhật lại xem có nhảy sang Quest tiếp theo trong chuỗi hay không
        UpdateDialogData();
    }
}
