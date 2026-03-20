using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabsController : MonoBehaviour
{
    public Image[] TabsImage;
    public GameObject[] PagesContent;
    // Start is called before the first frame update
    void Start()
    {
        ActiveTabs(0);
    }
    public void ActiveTabs(int tabNo)
    {
        for (int i = 0; i < PagesContent.Length; i++)
        {
            PagesContent[i].SetActive(false);
            TabsImage[i].color = Color.grey;
        }
        PagesContent[tabNo].SetActive(true);
        TabsImage[tabNo].color = Color.white;
    }
}
