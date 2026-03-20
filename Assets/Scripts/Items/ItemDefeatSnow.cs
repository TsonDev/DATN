using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemDefeatSnow : MonoBehaviour
{
    [SerializeField] float timeSnow = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController controller = collision.GetComponent<PlayerController>();

            if (controller != null)
            {
                controller.ActivateNatureResistant(timeSnow);
                Destroy(gameObject);
            }
        }
    }
}
