using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    public static DialogController instance { get; private set; }
    public GameObject npcDialogPanel;
    public TMP_Text dialogText, nameText;
    public Image portraitImage;

    public Transform ChoiceContainer;
    public GameObject ChoiceButtonPrefab;

    private void Awake()
    {
        if(instance == null) instance = this;
        else
        {
            Destroy(gameObject);
        }
    }
    public void ShowDialogUI(bool show)
    {
        npcDialogPanel.SetActive(show);
    }
    public void SetNpcInfor(string name, Sprite portrait)
    {
        nameText.text = name;
        portraitImage.sprite = portrait;
    }
    public void SetDialogText(string text)
    {
        dialogText.text = text;
    }
    public void ClearCHoices()
    {
        foreach (Transform child in ChoiceContainer)
        {
            Destroy(child.gameObject);
        }
    }
    public GameObject CreateButtonChoice(string choiceText, UnityEngine.Events.UnityAction onclick)
    {
        GameObject buttonChoice = GameObject.Instantiate(ChoiceButtonPrefab,ChoiceContainer);
        buttonChoice.GetComponentInChildren<TMP_Text>().text=choiceText;
        buttonChoice.GetComponent<Button>().onClick.AddListener(onclick);
        return buttonChoice;

    }
}
