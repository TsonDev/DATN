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

        GameObject prefab = ChoosePrefab();
        Vector3 pos = ChoosePosition();
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

    private Vector3 ChoosePosition()
    {
        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            var t = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            return t.position;
        }

        if (useArea)
        {
            Vector2 half = areaSize * 0.5f;
            float x = UnityEngine.Random.Range(-half.x, half.x);
            float y = UnityEngine.Random.Range(-half.y, half.y);
            return (Vector2)transform.position + new Vector2(x, y);
        }

        return transform.position;
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