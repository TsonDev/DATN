using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScenceFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine running;

    void Awake()
    {
        // đảm bảo có Image
        if (fadeImage != null)
        {
            // ban đầu trong suốt
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;

            // không bắt raycast để khỏi chặn click UI
            fadeImage.raycastTarget = false;
        }
    }

    public void FadeIn()
    {
        StartFadeTo(0f);
    }

    public void FadeOut()
    {
        StartFadeTo(1f);
    }

    public void StartFadeTo(float targetAlpha)
    {
        if (fadeImage == null) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeTo(targetAlpha));
    }

    public IEnumerator FadeTo(float targetAlpha)
    {
        var c = fadeImage.color;

        while (!Mathf.Approximately(c.a, targetAlpha))
        {
            Debug.Log("FadeTo target=" + targetAlpha);
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            fadeImage.color = c;
            yield return null;
        }
    }
}
