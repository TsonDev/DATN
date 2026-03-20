using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static DameType;

public class DamgeZone : MonoBehaviour
{
    [SerializeField] int amount;
   
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ChangeHealth(-amount, DameType.TypeDamage.Trap);
            }
        }
    }
}
