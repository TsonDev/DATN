using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEvents : MonoBehaviour
{
	GameController gameController;
    // Hàm này để load scene khác theo index (gán trong Build Settings)
    public void LoadScene(int index)
	{
       

        SceneManager.LoadScene(index);
    }
    public void Newgame(int index)
    {
        GameController.isNewGame = true;

        string path1 = Application.persistentDataPath + "/saveData.json";
        string path2 = Application.persistentDataPath + "/questProgress.json";

        if (File.Exists(path1)) File.Delete(path1);
        if (File.Exists(path2)) File.Delete(path2);

        Debug.Log("ĐÃ XÓA SAVE");

        SceneManager.LoadScene(index);
    }
    public void ButtonMusic()
	{
		if (AudioManager_Fight.instance != null)
			AudioManager_Fight.instance.PlayButton();
	}
	// 🔹 Hàm này để thoát game
	public void QuitGame()
	{
		Debug.Log("Thoát game..."); // chỉ hiển thị khi đang chạy trong Editor

		// Khi build ra file .exe hoặc .apk, dòng dưới sẽ thực sự thoát game
		Application.Quit();

#if UNITY_EDITOR
		// Nếu bạn đang test trong Unity Editor thì cần thêm dòng này
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}
