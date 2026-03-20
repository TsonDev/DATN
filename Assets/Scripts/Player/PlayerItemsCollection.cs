using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemsCollection : MonoBehaviour
{
    [SerializeField] SoundData soundEff;
    private InventoryController inventoryController;
    // Start is called before the first frame update
    void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null)
            {
                //Add inventory
               bool Added =  inventoryController.AddItem(collision.gameObject);
                SoundManager.Instance.PlaySound(soundEff);
                if (Added)
                {
                    Destroy(collision.gameObject);
                } 
            }
        }
    }
}
