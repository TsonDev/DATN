using UnityEngine;

public class BattleManager : MonoBehaviour
{
	[Header("Spawn Points")]
	public Transform playerSpawn;
	public Transform enemySpawn;

	[Header("Prefab Player/Enemy")]
	public GameObject[] playerPrefabs;
	public GameObject[] enemyPrefabs;

	[Header("Map Prefabs")]
	public GameObject[] mapPrefabs; // list map tương ứng với MapSelectScene

	[Header("Match Timer")]
	public MatchTimer matchTimer; // gán từ Inspector

	private GameObject currentMap;

	void Start()
	{
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
		int playerIndex = PlayerPrefs.GetInt("SelectedPlayerIndex", 0);
		int enemyIndex = PlayerPrefs.GetInt("SelectedEnemyIndex", 0);

		// kiểm tra index hợp lệ
		if (playerPrefabs == null || playerPrefabs.Length == 0 || playerIndex >= playerPrefabs.Length) playerIndex = 0;
		if (enemyPrefabs == null || enemyPrefabs.Length == 0 || enemyIndex >= enemyPrefabs.Length) enemyIndex = 0;

		GameObject player = Instantiate(playerPrefabs[playerIndex], playerSpawn.position, Quaternion.identity);
		GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], enemySpawn.position, Quaternion.identity);

		Debug.Log($"Spawned Player: {player.name} | Enemy: {enemy.name}");

		// 🔹 Gán Damageable cho MatchTimer
		if (matchTimer != null)
		{
			matchTimer.player = player.GetComponent<Damageable>();
			matchTimer.enemy = enemy.GetComponent<Damageable>();
		}
	}
}
