using System.Collections;
using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public float speed = 1f;
    public Vector2 direction = new Vector2(1, 0.5f);

    public float jitterAmount = 0.05f;
    public float jitterSpeed = 5f;

    public float lifeTime = 5f;
    public float fadeDuration = 1f;

    public float respawnDelayMin = 1f;
    public float respawnDelayMax = 3f;

    Vector3 startPos;
    float timer;

    SpriteRenderer sr;
    bool isRespawning;

    float maxAlpha = 115f / 255f;

    void Start()
    {
        startPos = transform.position;
        timer = Random.Range(0, lifeTime); // lệch pha giữa các cloud
        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isRespawning) return;

        // di chuyển chéo
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        // jitter
        float jitterX = Mathf.PerlinNoise(Time.time * jitterSpeed, 0) - 0.5f;
        float jitterY = Mathf.PerlinNoise(0, Time.time * jitterSpeed) - 0.5f;

        transform.position += new Vector3(jitterX, jitterY, 0) * jitterAmount;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            StartCoroutine(FadeOutAndRespawn());
        }
    }

    IEnumerator FadeIn()
    {
        Color c = sr.color;
        c.a = 0;
        sr.color = c;

        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, maxAlpha, t / fadeDuration);
            sr.color = c;
            yield return null;
        }

        c.a = maxAlpha;
        sr.color = c;
    }

    IEnumerator FadeOutAndRespawn()
    {
        isRespawning = true;

        float t = 0;
        Color c = sr.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0, t / fadeDuration);
            sr.color = c;
            yield return null;
        }

        // delay random để cloud không spawn cùng lúc
        float delay = Random.Range(respawnDelayMin, respawnDelayMax);
        yield return new WaitForSeconds(delay);

        transform.position = startPos;
        timer = lifeTime;

        StartCoroutine(FadeIn());

        isRespawning = false;
    }
}