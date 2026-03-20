using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menu;
    bool isPause;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("MenuController running");
        menu.SetActive(false);
        ResumeGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPause) {
                ResumeGame();
            }
            else PauseGame();
            /*Debug.Log("press tab");
            menu.SetActive(!menu.activeSelf);*/
        }
    }
    public void ResumeGame()
    {
        isPause = false;
        menu.SetActive(false);
        Time.timeScale = 1f;
    }
    public void PauseGame()
    {
        isPause = true;
        menu.SetActive(true);
        Time.timeScale = 0f;
    }
}
