using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            Animator animator = collision.GetComponent <Animator>();
            player.boostSpeed *= 2f;
            animator.speed = 0.5f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            Animator animator = collision.GetComponent<Animator>();
            player.boostSpeed /= 2f;
            animator.speed = 1f;
        }
    }
}
