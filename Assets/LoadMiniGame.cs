using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMiniGame : MonoBehaviour
{
    SceneManager sceneManager;
    public int indexSceneLoad;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            SceneManager.LoadScene(indexSceneLoad);
        }
    }
}
