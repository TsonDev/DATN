using System.Collections;
using Cinemachine;
using UnityEngine;

public class TelePointScript : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundry;
    [SerializeField] private Transform newPositionPlayer;
    [SerializeField] private ScenceFader fader;

    private CinemachineConfiner2D confiner;
    private CinemachineBrain brain;
    private bool isTeleporting;

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();

        if (Camera.main != null)
            brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTeleporting) return;
        if (!collision.CompareTag("Player")) return;
     
        

        StartCoroutine(TeleportWithFade(collision.transform));
    }

    private IEnumerator TeleportWithFade(Transform player)
    {
        isTeleporting = true;

        // Lấy vcam đang active thật (đỡ bị FindAnyObjectByType() lấy nhầm)
        CinemachineVirtualCamera vcam = null;
        if (brain != null && brain.ActiveVirtualCamera != null)
            vcam = brain.ActiveVirtualCamera.VirtualCameraGameObject
                .GetComponent<CinemachineVirtualCamera>();

        // Đảm bảo follow đúng player
        if (vcam != null)
            vcam.Follow = player;

        // 1) Fade out
        if (fader != null)
            yield return fader.FadeTo(1f);

        if (newPositionPlayer == null)
        {
            if (fader != null) yield return fader.FadeTo(0f);
            isTeleporting = false;
            yield break;
        }

        Vector3 oldPos = player.position;
        Vector3 targetPos = newPositionPlayer.position;

        // 2) Teleport: ưu tiên Rigidbody2D để không bị physics kéo ngược
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        RigidbodyInterpolation2D oldInterp = RigidbodyInterpolation2D.None;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            oldInterp = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation2D.None;

            rb.position = targetPos;
        }
        else
        {
            player.position = targetPos;
        }

        Physics2D.SyncTransforms();

        Vector3 delta = targetPos - oldPos;
        //update map area sau khi tele
        MapController.Instance?.UpdateCurrentArea(mapBoundry.name);

        // 3) Đổi confiner boundary
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = mapBoundry;
            confiner.InvalidateCache();
        }

        // 4) Snap camera theo teleport
        if (vcam != null)
        {
            vcam.OnTargetObjectWarped(player, delta);
            vcam.PreviousStateIsValid = false; // cực quan trọng: snap, không lerp
        }

        // Đợi 1 frame để confiner/brain settle
        yield return null;

        // Ép brain update ngay frame này (để trước khi fade in cam đã đúng chỗ)
        if (brain != null)
            brain.ManualUpdate();

        // restore interpolation
        if (rb != null)
            rb.interpolation = oldInterp;

        // 5) Fade in
        if (fader != null)
            yield return fader.FadeTo(0f);

        isTeleporting = false;
    }
}