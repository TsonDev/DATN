using System.Collections;
using UnityEngine;
using Cinemachine;

public class UltimateEffect : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public CinemachineVirtualCamera vcam;
    public LetterboxEffect letterbox;

    [Header("Zoom")]
    public float zoomSize = 3f;
    public float zoomSpeed = 3f;

    [Header("Timing")]
    public float holdTime = 0.4f;

    [Header("Effect")]
    public GameObject effectPrefab;
    public float timePrefab = 1f;

    private float originalSize;
    CinemachineConfiner confiner;
    private Transform originalFollow;

    void Start()
    {
        if (vcam != null)
            originalSize = vcam.m_Lens.OrthographicSize;
        confiner = vcam.GetComponent<CinemachineConfiner>();
    }

    public void PlayUltimate()
    {
        StartCoroutine(UltiRoutine());
    }

    IEnumerator UltiRoutine()
    {
        // bật letterbox
        if (letterbox != null)
            yield return StartCoroutine(letterbox.Show()); 
        if (confiner != null)
            confiner.enabled = false;
        // lưu trạng thái cũ
        originalFollow = vcam.Follow;

        // follow player tạm thời
        vcam.Follow = player;

        // FIX camera center bằng code
        ForceCenterCamera();

        // ép Cinemachine update lại
        vcam.PreviousStateIsValid = false;

        // chờ 1 frame để apply
        yield return null;

        // đảm bảo camera nằm đúng player (hard fix)
        vcam.transform.position = new Vector3(
            player.position.x,
            player.position.y,
            vcam.transform.position.z
        );

        // zoom vào
        yield return StartCoroutine(ZoomIn());

        // giữ
        yield return new WaitForSecondsRealtime(holdTime);

        // spawn effect
        if (effectPrefab != null && player != null)
        {
           GameObject eff= Instantiate(effectPrefab, player.position, Quaternion.identity);
            Destroy(eff, timePrefab);
        }

        yield return new WaitForSecondsRealtime(0.2f);

        // zoom ra
        yield return StartCoroutine(ZoomOut());
          if (confiner != null)
        confiner.enabled = true;
        // trả lại trạng thái ban đầu
        vcam.Follow = originalFollow;

        if (letterbox != null)
            yield return StartCoroutine(letterbox.Hide());
    }

    void ForceCenterCamera()
    {
        var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (transposer != null)
        {
            transposer.m_ScreenX = 0.5f;
            transposer.m_ScreenY = 0.5f;

            transposer.m_TrackedObjectOffset = Vector3.zero;

            transposer.m_DeadZoneWidth = 0f;
            transposer.m_DeadZoneHeight = 0f;

            transposer.m_SoftZoneWidth = 0f;
            transposer.m_SoftZoneHeight = 0f;

            transposer.m_XDamping = 0f;
            transposer.m_YDamping = 0f;
        }
    }

    IEnumerator ZoomIn()
    {
        float t = 0;
        float start = vcam.m_Lens.OrthographicSize;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * zoomSpeed;
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(start, zoomSize, t);
            yield return null;
        }
    }

    IEnumerator ZoomOut()
    {
        float t = 0;
        float start = vcam.m_Lens.OrthographicSize;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * zoomSpeed;
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(start, originalSize, t);
            yield return null;
        }
    }
}