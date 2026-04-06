using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MatchTimer : MonoBehaviour
{
	[Header("Timer Settings")]
	public float matchTime = 120f;
	private float timeRemaining;
	private bool matchEnded = false;

	[Header("Start Countdown Settings")]
	public TextMeshProUGUI startCountdownText;
	public float startCountdownDuration = 3f;

	[Header("Countdown Images")]
	public Image count1Image;
	public Image count2Image;
	public Image count3Image;
	public Image fightImage;

	[Header("References")]
	public TextMeshProUGUI timerText;
	public Image resultPanel;

	[Header("Kết quả bằng hình ảnh")]
	public Image victoryImage;
	public Image loseImage;
	public Image drawImage;

	[Header("Marker Prefabs (World Objects)")]
	public GameObject playerMarkerPrefab;
	public GameObject enemyMarkerPrefab;
	public Vector3 markerOffset = new Vector3(0, 2f, 0);

	public Damageable player;
	public Damageable enemy;

	private GameObject playerMarker;
	private GameObject enemyMarker;

	private void Start()
	{
		timeRemaining = matchTime;
		UpdateTimerText();

		// Ẩn UI kết quả khi bắt đầu
		resultPanel.gameObject.SetActive(false);
		victoryImage.gameObject.SetActive(false);
		loseImage.gameObject.SetActive(false);
		drawImage.gameObject.SetActive(false);

		// Ẩn các ảnh đếm ngược
		if (count1Image) count1Image.gameObject.SetActive(false);
		if (count2Image) count2Image.gameObject.SetActive(false);
		if (count3Image) count3Image.gameObject.SetActive(false);
		if (fightImage) fightImage.gameObject.SetActive(false);

		Time.timeScale = 0f;
		StartCoroutine(StartCountdown());
	}

	private IEnumerator StartCountdown()
	{
		// Ẩn text cũ
		if (startCountdownText) startCountdownText.gameObject.SetActive(false);

		// Hiện lần lượt ảnh 3 → 2 → 1 → FIGHT
		if (count3Image)
		{
			count3Image.gameObject.SetActive(true);
			yield return new WaitForSecondsRealtime(1f);
			count3Image.gameObject.SetActive(false);
		}

		if (count2Image)
		{
			count2Image.gameObject.SetActive(true);
			yield return new WaitForSecondsRealtime(1f);
			count2Image.gameObject.SetActive(false);
		}

		if (count1Image)
		{
			count1Image.gameObject.SetActive(true);
			yield return new WaitForSecondsRealtime(1f);
			count1Image.gameObject.SetActive(false);
		}

		if (fightImage)
		{
			fightImage.gameObject.SetActive(true);
			AudioManager_Fight.instance.VoiceFight();
			yield return new WaitForSecondsRealtime(1f);
			fightImage.gameObject.SetActive(false);
		}

		// ✅ Spawn marker khi trận bắt đầu
		if (playerMarkerPrefab != null && player != null)
			playerMarker = Instantiate(playerMarkerPrefab, player.transform.position + markerOffset, Quaternion.identity);

		if (enemyMarkerPrefab != null && enemy != null)
			enemyMarker = Instantiate(enemyMarkerPrefab, enemy.transform.position + markerOffset, Quaternion.identity);

		Time.timeScale = 1f;
	}

	// Các phần còn lại giữ nguyên hoàn toàn
	private void Update()
	{
		if (matchEnded || Time.timeScale == 0f) return;

		timeRemaining -= Time.deltaTime;
		UpdateTimerText();

		UpdateMarkers();

		if (timeRemaining <= 0f)
		{
			timeRemaining = 0f;
			EndMatch();
		}

		if (player.Health <= 0 && enemy.Health <= 0)
			EndMatch("DRAW");
		else if (player.Health <= 0)
			EndMatch("LOSE");
		else if (enemy.Health <= 0)
			EndMatch("VICTORY");
	}

	private void UpdateTimerText()
	{
		int minutes = Mathf.FloorToInt(timeRemaining / 60);
		int seconds = Mathf.FloorToInt(timeRemaining % 60);
		timerText.text = $"{minutes:00}:{seconds:00}";
	}

	private void UpdateMarkers()
	{
		if (playerMarker != null && player != null)
			playerMarker.transform.position = player.transform.position + markerOffset;

		if (enemyMarker != null && enemy != null)
			enemyMarker.transform.position = enemy.transform.position + markerOffset;
	}

	private void EndMatch(string forceResult = "")
	{
		if (matchEnded) return;
		matchEnded = true;

		resultPanel.gameObject.SetActive(true);
		victoryImage.gameObject.SetActive(false);
		loseImage.gameObject.SetActive(false);
		drawImage.gameObject.SetActive(false);

		string result;

		if (forceResult != "")
		{
			result = forceResult;
		}
		else
		{
			int playerHealth = player.Health;
			int enemyHealth = enemy.Health;

			if (playerHealth > enemyHealth) result = "VICTORY";
			else if (enemyHealth > playerHealth) result = "LOSE";
			else result = "DRAW";
		}

		switch (result)
		{
			case "VICTORY":
				victoryImage.gameObject.SetActive(true);
				break;
			case "LOSE":
				loseImage.gameObject.SetActive(true);
				break;
			case "DRAW":
				drawImage.gameObject.SetActive(true);
				break;
		}

		if (playerMarker != null) Destroy(playerMarker);
		if (enemyMarker != null) Destroy(enemyMarker);

		StartCoroutine(ReturnToMenuAfterDelay());
	}

	private IEnumerator DelayStopTime()
	{
		yield return new WaitForSeconds(1f);
		Time.timeScale = 0f;
	}
	private IEnumerator ReturnToMenuAfterDelay()
	{
		// 🔹 Tạm dừng thời gian để người chơi xem kết quả, nhưng vẫn cho UI hiển thị
		Time.timeScale = 0f;

		// 🔹 Chờ 5 giây thật (Realtime)
		yield return new WaitForSecondsRealtime(2f);

		// 🔹 Bật lại thời gian (phòng khi cần hiệu ứng chuyển)
		Time.timeScale = 1f;

		// 🔹 Quay về menu chính
		UnityEngine.SceneManagement.SceneManager.LoadScene("MenuFT");
	}

}
