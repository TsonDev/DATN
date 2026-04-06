using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
	[Header("Danh sách Prefab Player")]
	public GameObject[] playerPrefabs;

	[Header("Danh sách Prefab Enemy (AI)")]
	public GameObject[] enemyPrefabs;

	[Header("Scene Index")]
	[Tooltip("Index của scene Battle trong Build Settings")]
	public int battleSceneIndex = 1;
	[Tooltip("Index của scene Main Menu trong Build Settings")]
	public int mainMenuSceneIndex = 0;

	[Header("UI Labels")]
	public GameObject playerLabelUI; // Hình ảnh/ chữ "PLAYER" trên Canvas
	public GameObject enemyLabelUI;  // Hình ảnh/ chữ "ENEMY" trên Canvas

	private int currentSelectedCharacterIndex = -1; // nhân vật đang được click ảnh
	private int selectedPlayerIndex = -1; // nhân vật được chọn làm Player
	private int selectedEnemyIndex = -1; // nhân vật được chọn làm Enemy

	void Start()
	{
		// Ẩn label khi bắt đầu
		playerLabelUI.SetActive(false);
		enemyLabelUI.SetActive(false);
	}

	// 🟢 Gọi khi click vào ảnh nhân vật trong UI
	public void SelectCharacter(int index)
	{
		currentSelectedCharacterIndex = index;
		Debug.Log("Đang chọn nhân vật số: " + index + " - " + playerPrefabs[index].name);
	}

	// 🟠 Khi ấn vào nút "Chọn làm Player"
	public void ChooseAsPlayer()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning("⚠️ Bạn chưa chọn nhân vật nào để gán làm Player!");
			return;
		}

		selectedPlayerIndex = currentSelectedCharacterIndex;
		Debug.Log("✅ Đã chọn " + playerPrefabs[selectedPlayerIndex].name + " làm Player!");

		// Hiển thị label PLAYER, ẩn label ENEMY
		playerLabelUI.SetActive(true);
		enemyLabelUI.SetActive(false);
	}

	// 🔵 Khi ấn vào nút "Chọn làm Enemy"
	public void ChooseAsEnemy()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning("⚠️ Bạn chưa chọn nhân vật nào để gán làm Enemy!");
			return;
		}

		selectedEnemyIndex = currentSelectedCharacterIndex;
		Debug.Log("✅ Đã chọn " + enemyPrefabs[selectedEnemyIndex].name + " làm Enemy!");

		// Hiển thị label ENEMY, ẩn label PLAYER
		playerLabelUI.SetActive(false);
		enemyLabelUI.SetActive(true);
	}

	// 🔥 Khi nhấn nút Fight
	public void StartBattle()
	{
		if (selectedPlayerIndex < 0 || selectedEnemyIndex < 0)
		{
			Debug.LogWarning("⚠️ Chưa chọn đủ player hoặc enemy!");
			return;
		}

		// Lưu index sang scene đấu
		PlayerPrefs.SetInt("SelectedPlayerIndex", selectedPlayerIndex);
		PlayerPrefs.SetInt("SelectedEnemyIndex", selectedEnemyIndex);
		PlayerPrefs.Save();

		SceneManager.LoadScene(battleSceneIndex);
	}

	// ⏹ Thoát ra menu chính
	public void QuitGame()
	{
		SceneManager.LoadScene(mainMenuSceneIndex);
	}
}
