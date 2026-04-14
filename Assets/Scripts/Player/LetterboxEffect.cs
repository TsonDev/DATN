using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LetterboxEffect : MonoBehaviour
{
    public RectTransform topBar;
    public RectTransform bottomBar;

    public float targetHeight = 150f;
    public float speed = 5f;

    public IEnumerator Show()
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * speed;
            float h = Mathf.Lerp(0, targetHeight, t);

            topBar.sizeDelta = new Vector2(0, h);
            bottomBar.sizeDelta = new Vector2(0, h);

            yield return null;
        }
    }

    public IEnumerator Hide()
    {
        float t = 0;
        float start = topBar.sizeDelta.y;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * speed;
            float h = Mathf.Lerp(start, 0, t);

            topBar.sizeDelta = new Vector2(0, h);
            bottomBar.sizeDelta = new Vector2(0, h);

            yield return null;
        }
    }
}