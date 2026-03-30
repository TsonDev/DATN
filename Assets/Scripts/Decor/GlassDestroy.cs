using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassDestroy : MonoBehaviour
{
    Animator animator;
    public SoundData soundSource;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MeleeWeapon meleeWeapon = collision.GetComponent<MeleeWeapon>();
        ProjectTile projectTile = collision.GetComponent<ProjectTile>();
        if (meleeWeapon != null || projectTile != null)
        {
            // Start destroy sequence instead of destroying immediately
            StartCoroutine(PlayDestroyAndRemove());
        }
    }

    private IEnumerator PlayDestroyAndRemove()
    {
        if (animator != null)
        {
            animator.SetBool("isDestroy", true);
           
        }
        SoundManager.Instance.PlaySound(soundSource);
        // disable collider(s) so it won't retrigger
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        // try to get clip length for the destroy animation
        float waitTime = 0.5f; // fallback
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                var nameLower = clip.name.ToLower();
                if (nameLower.Contains("destroy") || nameLower.Contains("break") || nameLower.Contains("shatter"))
                {
                    waitTime = clip.length;
                    break;
                }
            }
        }

        // wait one frame to allow Animator to transition, then wait clip length
        yield return null;
        yield return new WaitForSeconds(waitTime);

        Destroy(gameObject);
    }
}
