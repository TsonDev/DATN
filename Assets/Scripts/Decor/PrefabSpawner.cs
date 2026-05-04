using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> prefabs = new();      // list các prefab có thể spawn
    public bool randomPrefab = true;              // chọn prefab ngẫu nhiên hay tuần tự

    [Header("Timing")]
    public bool spawnOnStart = true;
    public float startDelay = 0f;
    public float spawnInterval = 5f;              // khoảng thời gian cố định
    public bool useRandomInterval = false;
    public float randomIntervalMin = 1f;
    public float randomIntervalMax = 5f;

    [Header("Spawn options")]
    public bool useSpawnPoints = false;
    public Transform[] spawnPoints;                // nếu có thì spawn tại một trong các transform này
    public bool useArea = false;                   // nếu true dùng areaSize làm vùng spawn (center = this.transform.position)
    public Vector2 areaSize = Vector2.one;

    [Header("Overlap Prevention")]
    public bool avoidOverlap = false;              // Nếu true, sẽ tránh spawn đè lên vật khác
    public float overlapRadius = 0.5f;             // Bán kính kiểm tra va chạm
    public LayerMask overlapMask = -1;             // Layer để kiểm tra va chạm
    public int maxSpawnAttempts = 10;              // Số lần thử tìm vị trí trống hợp lệ trước khi bỏ qua

    [Header("Limits & parenting")]
    public int maxConcurrent = 0;                  // 0 = unlimited
    public bool parentToSpawner = true;

    [Header("Behavior")]
    public bool loop = true;

    // runtime
    public event Action<GameObject> OnSpawn;
    private readonly List<GameObject> _spawned = new();
    private Coroutine _spawnRoutine;
    private int _nextIndex = 0;

    void Start()
    {
        if (spawnOnStart) StartSpawning();
    }

    void OnDisable()
    {
        StopSpawning();
    }

    public void StartSpawning()
    {
        if (_spawnRoutine != null) return;
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    public GameObject SpawnOnce()
    {
        if (prefabs == null || prefabs.Count == 0) return null;

        // Enforce maxConcurrent
        if (maxConcurrent > 0 && _spawned.Count >= maxConcurrent)
        {
            return null;
        }

        if (!TryChoosePosition(out Vector3 pos))
        {
            return null; // Không tìm được vị trí phù hợp, bỏ qua spawn lần này
        }

        GameObject prefab = ChoosePrefab();
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        if (parentToSpawner) go.transform.SetParent(transform);

        _spawned.Add(go);
        // remove from list when destroyed
        var tracker = go.AddComponent<SpawnedTracker>();
        tracker.Init(() => _spawned.Remove(go));

        OnSpawn?.Invoke(go);
        return go;
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (loop)
        {
            if (maxConcurrent == 0 || _spawned.Count < maxConcurrent)
            {
                SpawnOnce();
            }

            float wait = GetNextInterval();
            yield return new WaitForSeconds(wait);
        }

        _spawnRoutine = null;
    }

    private float GetNextInterval()
    {
        if (useRandomInterval)
        {
            return UnityEngine.Random.Range(randomIntervalMin, randomIntervalMax);
        }
        return Mathf.Max(0.01f, spawnInterval);
    }

    private GameObject ChoosePrefab()
    {
        if (!randomPrefab)
        {
            var prefab = prefabs[_nextIndex % prefabs.Count];
            _nextIndex++;
            return prefab;
        }
        return prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
    }

    private bool TryChoosePosition(out Vector3 pos)
    {
        pos = transform.position;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 tempPos = transform.position;

            if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
            {
                tempPos = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position;
            }
            else if (useArea)
            {
                Vector2 half = areaSize * 0.5f;
                float x = UnityEngine.Random.Range(-half.x, half.x);
                float y = UnityEngine.Random.Range(-half.y, half.y);
                tempPos = (Vector2)transform.position + new Vector2(x, y);
            }

            if (!avoidOverlap)
            {
                pos = tempPos;
                return true;
            }

            // Kiểm tra va chạm (Overlap check)
            Collider2D col = Physics2D.OverlapCircle(tempPos, overlapRadius, overlapMask);
            if (col == null)
            {
                pos = tempPos;
                return true;
            }
        }

        return false; // Đã thử maxSpawnAttempts lần nhưng không tìm được chỗ trống
    }

    private void OnDrawGizmosSelected()
    {
        if (useArea)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0));
        }

        if (avoidOverlap)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, overlapRadius);
        }
    }

    // Remove all spawned (optional)
    public void ClearSpawned(bool destroy = true)
    {
        if (destroy)
        {
            foreach (var go in new List<GameObject>(_spawned))
            {
                if (go != null) Destroy(go);
            }
        }
        _spawned.Clear();
    }

    // small helper component to notify spawner when an object is destroyed
    class SpawnedTracker : MonoBehaviour
    {
        private Action _onDestroyed;
        public void Init(Action onDestroyed) => _onDestroyed = onDestroyed;
        void OnDestroy() => _onDestroyed?.Invoke();
    }
}