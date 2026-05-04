using UnityEngine;

public class SimpleAreaSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject prefabToSpawn;             // Prefab cần sinh ra (Quái, rương, item,...)
    public Vector2 spawnAreaSize = new Vector2(5f, 5f); // Kích thước vùng spawn

    [Header("Spawn Mode")]
    public bool spawnOnStart = true;             // Có tự động sinh ra khi bắt đầu game không?
    public int spawnCount = 1;                   // Số lượng sinh ra cùng lúc (Nếu spawnInterval = 0)
    public float spawnInterval = 0f;             // Thời gian giữa mỗi lần sinh (0 = sinh tất cả cùng lúc)

    private void Start()
    {
        if (spawnOnStart)
        {
            if (spawnInterval > 0)
            {
                InvokeRepeating(nameof(SpawnRandom), 0f, spawnInterval);
            }
            else
            {
                // Gọi 1 lần vì bên trong SpawnRandom đã có vòng lặp spawnCount
                SpawnRandom();
            }
        }
    }

    public void SpawnRandom()
    {
        if (prefabToSpawn == null) return;

        // Sinh ra 'spawnCount' object mỗi lần gọi hàm thay vì chỉ 1
        for (int i = 0; i < spawnCount; i++)
        {
            // Random vị trí trong khoảng areaSize
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomY = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            
            Vector3 randomPos = transform.position + new Vector3(randomX, randomY, 0f);

            // Sinh ra object
            Instantiate(prefabToSpawn, randomPos, Quaternion.identity, transform);
        }
    }

    // Vẽ khung xanh trên Scene để dễ căn chỉnh vùng spawn
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0));
    }
}
