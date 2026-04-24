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
	public GameObject playerLabelUI;   // Label hiện trên nhân vật được chọn làm P1
	public GameObject player2LabelUI;  // Label hiện trên nhân vật được chọn làm P2
	public GameObject enemyLabelUI;    // Label hiện trên nhân vật được chọn làm Enemy

	private int currentSelectedCharacterIndex = -1;
	private int selectedPlayerIndex  = -1;
	private int selectedPlayer2Index = -1; // -1 = chưa chọn P2 (PvAI mode)
	private int selectedEnemyIndex   = -1;



	// ─── Chọn nhân vật ────────────────────────────────────────────────────────

	// Gọi khi click vào ảnh nhân vật trong UI (truyền index từ Inspector)
	public void SelectCharacter(int index)
	{
		currentSelectedCharacterIndex = index;
		Debug.Log("Đang chọn nhân vật số: " + index + " - " + playerPrefabs[index].name);
	}

	//  Nút "Chọn làm Player 1"
	public void ChooseAsPlayer()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning(" Bạn chưa chọn nhân vật nào để gán làm Player 1!");
			return;
		}

		selectedPlayerIndex = currentSelectedCharacterIndex;
		Debug.Log("✅ Đã chọn " + playerPrefabs[selectedPlayerIndex].name + " làm Player 1!");

		playerLabelUI?.SetActive(true);
	}

	// Nút "Chọn làm Player 2" — tự động bật PvP mode
	public void ChooseAsPlayer2()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning("⚠️ Bạn chưa chọn nhân vật nào để gán làm Player 2!");
			return;
		}

		selectedPlayer2Index = currentSelectedCharacterIndex;
		// Player 2 dùng cùng slot SelectedEnemyIndex để BattleManager đọc
		selectedEnemyIndex = selectedPlayer2Index;

		Debug.Log(" Đã chọn " + playerPrefabs[selectedPlayer2Index].name + " làm Player 2! (PvP mode tự động bật)");

		player2LabelUI?.SetActive(true);
	}

	//  Nút "Chọn làm Enemy" — tự động bật PvAI mode
	public void ChooseAsEnemy()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning(" Bạn chưa chọn nhân vật nào để gán làm Enemy!");
			return;
		}

		selectedEnemyIndex   = currentSelectedCharacterIndex;
		selectedPlayer2Index = -1; // reset P2 → báo PvAI

		Debug.Log(" Đã chọn " + enemyPrefabs[selectedEnemyIndex].name + " làm Enemy (AI)!");

		enemyLabelUI?.SetActive(true);
	}

	// ─── Bắt đầu trận ─────────────────────────────────────────────────────────

	public void StartBattle()
	{
		if (selectedPlayerIndex < 0 || selectedEnemyIndex < 0)
		{
			Debug.LogWarning(" Chưa chọn đủ nhân vật! Cần chọn Player 1 và (Player 2 hoặc Enemy).");
			return;
		}

		// Xác định GameMode: nếu đã chọn P2 → PvP, ngược lại → PvAI
		GameMode mode = (selectedPlayer2Index >= 0) ? GameMode.PvP : GameMode.PvAI;

		PlayerPrefs.SetInt("SelectedPlayerIndex", selectedPlayerIndex);
		PlayerPrefs.SetInt("SelectedEnemyIndex",  selectedEnemyIndex);
		PlayerPrefs.SetInt("GameMode", (int)mode);
		PlayerPrefs.Save();

		Debug.Log($"[CharacterSelect] Bắt đầu: P1={selectedPlayerIndex}, Opponent={selectedEnemyIndex}, Mode={mode}");
		SceneManager.LoadScene(battleSceneIndex);
	}

	// ⏹ Thoát ra menu chính
	public void QuitGame()
	{
		SceneManager.LoadScene(mainMenuSceneIndex);
	}
}
