using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewNPCDialog", menuName ="NPC Dialog")]
public class NPCDialog : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogLines;
    public bool[] autoProgressLines;
    public bool[] EndProgressLines;
    public float autoProgressDisplay = 1.5f;
    public float typingSpeed = 0.05f;

    public DialogChoice[] choices;

    public int questInprogressIndex;//quest in progress
    public int questCompletedIndex;// completed quest
    public Quest quest;//npc give quest;

    [Space(8)]
    [Header("Load Scene")]
    [Tooltip("Nếu NPC này là đích đến cuối game, điền questID có objective CheckItemLoadScene vào đây. \nSau khi đọc xong hội thoại (trạng thái Completed) → kiểm tra item → nếu đủ mới load scene.")]
    public string loadSceneQuestID; // để trống nếu NPC này không trigger load scene
}
[System.Serializable]
public class DialogChoice {
    public int dialogIndex;
    public string[] choices;
    public int[] nextDialogIndexes;

    public bool[] giveQuest; //if choice give quest
}
