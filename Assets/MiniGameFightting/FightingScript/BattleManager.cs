using UnityEngine;
using UnityEngine.InputSystem;

public enum GameMode { PvAI, PvP }

public class BattleManager : MonoBehaviour
{
	[Header("Game Mode")]
	[Tooltip("PvAI = người vs máy (như cũ). PvP = 2 người trên cùng 1 bàn phím.")]
	public GameMode gameMode = GameMode.PvAI;

	[Header("Spawn Points")]
	public Transform playerSpawn;
	public Transform enemySpawn;

	[Header("Prefab Player/Enemy")]
	public GameObject[] playerPrefabs;
	public GameObject[] enemyPrefabs;

	[Header("PvP – Player 2")]
	[Tooltip("Prefab nhân vật dành cho Player 2 (PvP mode). Nếu để trống sẽ dùng chung playerPrefabs.")]
	public GameObject[] player2Prefabs;
	[Tooltip("Control Scheme tên trong PlayerInputActions dành cho Player 1.")]
	public string schemeP1 = "Keyboard&Mouse";
	[Tooltip("Control Scheme tên trong PlayerInputActions dành cho Player 2.")]
	public string schemeP2 = "Player2";

	[Header("Map Prefabs")]
	public GameObject[] mapPrefabs;

	[Header("Match Timer")]
	public MatchTimer matchTimer;

	private GameObject currentMap;

	void Start()
	{
		// Đọc GameMode từ màn CharacterSelect
		int savedMode = PlayerPrefs.GetInt("GameMode", 0);
		gameMode = (GameMode)savedMode;
		Debug.Log($"[BattleManager] GameMode từ PlayerPrefs = {savedMode} → {gameMode}");

		SpawnMap();
		SpawnCharacters();
	}

	void SpawnMap()
	{
		int mapIndex = PlayerPrefs.GetInt("SelectedMapIndex", 0);

		if (mapPrefabs == null || mapPrefabs.Length == 0)
		{
			Debug.LogWarning("[BattleManager] mapPrefabs rỗng, không spawn map.");
			return;
		}

		if (mapIndex < 0 || mapIndex >= mapPrefabs.Length) mapIndex = 0;

		currentMap = Instantiate(mapPrefabs[mapIndex], Vector3.zero, Quaternion.identity);
		Debug.Log("Spawned Map: " + currentMap.name);
	}

	void SpawnCharacters()
	{
		// ──────────── Đọc lựa chọn nhân vật từ CharacterSelect ────────────
		int p1Index = PlayerPrefs.GetInt("SelectedPlayerIndex", 0);
		int p2Index = PlayerPrefs.GetInt("SelectedEnemyIndex", 0);

		// Clamp index an toàn
		if (playerPrefabs == null || playerPrefabs.Length == 0) p1Index = 0;
		else p1Index = Mathf.Clamp(p1Index, 0, playerPrefabs.Length - 1);

		// ──────────────────────── SPAWN PLAYER 1 ────────────────────────
		GameObject p1 = Instantiate(playerPrefabs[p1Index], playerSpawn.position, Quaternion.identity);
		p1.tag = "Player";
		p1.name = "Player1";

		// Gán Control Scheme P1 nếu có PlayerInput component
		var p1Input = p1.GetComponent<PlayerInput>();
		if (p1Input != null && Keyboard.current != null)
		{
			p1Input.SwitchCurrentControlScheme(schemeP1, Keyboard.current);
			Debug.Log($"[BattleManager] P1 dùng scheme: {schemeP1}");
		}

		// ──────────────────────── SPAWN ĐỐI THỦ ────────────────────────
		if (gameMode == GameMode.PvP)
		{
			SpawnPlayer2(p2Index, p1.GetComponent<Damageable>());
		}
		else
		{
			SpawnAI(p2Index, p1.GetComponent<Damageable>());
		}
	}

	void SpawnPlayer2(int p2Index, Damageable p1Damageable)
	{
		// Chọn prefab P2: ưu tiên player2Prefabs, fallback sang playerPrefabs
		GameObject[] pool = (player2Prefabs != null && player2Prefabs.Length > 0)
			? player2Prefabs
			: playerPrefabs;

		if (pool == null || pool.Length == 0)
		{
			Debug.LogError("[BattleManager] Không có prefab nào cho Player 2!");
			return;
		}

		p2Index = Mathf.Clamp(p2Index, 0, pool.Length - 1);

		// Spawn tại vị trí enemySpawn, lật hướng để mặt về phía P1
		GameObject p2 = Instantiate(pool[p2Index], enemySpawn.position, Quaternion.identity);
		p2.tag = "Enemy";   // dùng tag có sẵn
		p2.name = "Player2";
		p2.transform.localScale = new Vector3(-Mathf.Abs(p2.transform.localScale.x),
		                                       p2.transform.localScale.y,
		                                       p2.transform.localScale.z);

		//  Tắt AIControllerFT nếu có
		var ai = p2.GetComponent<AIControllerFT>();
		if (ai != null)
		{
			ai.enabled = false;
			Debug.Log("[BattleManager] AIControllerFT tắt (PvP mode).");
		}

		//  Tắt PlayerInput nếu có — tránh P2 nhận input cùng scheme với P1
		//    (nếu không tắt, OnMove/OnJump của P2 sẽ phản ứng theo input của P1)
		var p2PlayerInput = p2.GetComponent<UnityEngine.InputSystem.PlayerInput>();
		if (p2PlayerInput != null)
		{
			p2PlayerInput.enabled = false;
			Debug.Log("[BattleManager] PlayerInput P2 tắt — dùng keyboard polling.");
		}

		// Bật PlayerControllerFT và đặt playerNumber = 2
		var p2Controller = p2.GetComponent<PlayerControllerFT>();
		if (p2Controller != null)
		{
			p2Controller.enabled = true;
			p2Controller.playerNumber = 2;

			// Sync hướng mặt: P2 spawn với scale âm (nhìn trái về phía P1)
			// → _isFacingRight phải là false để SetFacingDirection không flip ngược
			p2Controller.SetInitialFacingDirection(false);

			Debug.Log("[BattleManager] P2 bật: playerNumber=2, facing=LEFT.");
		}
		else
		{
			Debug.LogWarning("[BattleManager] Prefab P2 không có PlayerControllerFT!");
		}


		// Gán vào MatchTimer
		if (matchTimer != null)
		{
			matchTimer.player = p1Damageable;
			matchTimer.enemy = p2.GetComponent<Damageable>();
			Debug.Log("[BattleManager] PvP: matchTimer đã gán P1 và P2.");
		}

		Debug.Log($"[BattleManager] PvP Spawned → P2: {p2.name}");
	}

	void SpawnAI(int enemyIndex, Damageable p1Damageable)
	{
		if (enemyPrefabs == null || enemyPrefabs.Length == 0) enemyIndex = 0;
		else enemyIndex = Mathf.Clamp(enemyIndex, 0, enemyPrefabs.Length - 1);

		GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], enemySpawn.position, Quaternion.identity);
		enemy.tag = "Enemy";
		enemy.name = "Enemy_AI";

		Debug.Log($"[BattleManager] PvAI Spawned → Enemy: {enemy.name}");

		if (matchTimer != null)
		{
			matchTimer.player = p1Damageable;
			matchTimer.enemy = enemy.GetComponent<Damageable>();
		}
	}
}
