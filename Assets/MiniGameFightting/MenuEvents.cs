using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEvents : MonoBehaviour
{
	// Hàm này để load scene khác theo index (gán trong Build Settings)
	public void LoadScene(int index)
	{
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
