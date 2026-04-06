using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectManager : MonoBehaviour
{
	[Header("Danh sách Map UI")]
	public GameObject[] mapButtons;   // preview map trên UI
	public GameObject[] mapPrefabs;   // prefab map tương ứng để spawn ở BattleScene

	[Header("Scene Index")]
	public int battleSceneIndex = 7;  // scene battle
	public int mainMenuSceneIndex = 4; // scene menu chính

	private int selectedMapIndex;

	private void Awake()
	{
		// reset lại map index về 0 mỗi khi vào MapSelectScene
		PlayerPrefs.SetInt("SelectedMapIndex", 0);
		PlayerPrefs.Save();

		selectedMapIndex = 0;

		// ẩn tất cả map
		foreach (GameObject map in mapButtons)
		{
			map.SetActive(false);
		}

		// bật map đầu tiên
		if (mapButtons.Length > 0)
			mapButtons[selectedMapIndex].SetActive(true);
	}

	// Chọn map tiếp theo
	public void NextMap()
	{
		mapButtons[selectedMapIndex].SetActive(false);
		selectedMapIndex++;
		if (selectedMapIndex >= mapButtons.Length)
			selectedMapIndex = 0;

		mapButtons[selectedMapIndex].SetActive(true);
		PlayerPrefs.SetInt("SelectedMapIndex", selectedMapIndex);
		PlayerPrefs.Save();

		Debug.Log("Chọn map: " + mapPrefabs[selectedMapIndex].name);
	}

	// Chọn map trước đó
	public void PreviousMap()
	{
		mapButtons[selectedMapIndex].SetActive(false);
		selectedMapIndex--;
		if (selectedMapIndex < 0)
			selectedMapIndex = mapButtons.Length - 1;

		mapButtons[selectedMapIndex].SetActive(true);
		PlayerPrefs.SetInt("SelectedMapIndex", selectedMapIndex);
		PlayerPrefs.Save();

		Debug.Log("Chọn map: " + mapPrefabs[selectedMapIndex].name);
	}

	// Chuyển sang scene Battle
	public void StartBattle()
	{
		if (mapPrefabs.Length == 0)
		{
			Debug.LogWarning("Chưa có map nào để load!");
			return;
		}

		SceneManager.LoadScene(battleSceneIndex);
	}

	// Quay về menu
	public void QuitGame()
	{
		SceneManager.LoadScene(mainMenuSceneIndex);
	}
}
