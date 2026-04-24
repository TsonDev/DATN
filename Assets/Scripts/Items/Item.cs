using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;
    public int maxStack = 99;
    public TMP_Text quantityText;
    [Header("Shop item")]
    public int buyPrice;
    [Range(0f, 1f)]
    public float sellPriceMultiplier = 0.5f;// sell with 50% price of the buy price
    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantity();
    }
    public virtual void ShowPopUp()
    {
        Sprite itemIcon = GetComponent<SpriteRenderer>()?.sprite;
        if(ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }
    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiplier);
    }
    public void UpdateQuantity()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }
    public void AddtoStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantity();
    }
    public int RemoveFromStack(int amount = 1)
    {
        int romoved =Mathf.Min(amount, quantity);
        quantity-= romoved;
        UpdateQuantity();
        return romoved;
    }

    public GameObject CloneItem(int newQuatity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();
        cloneItem.quantity = newQuatity;
        cloneItem.UpdateQuantity();
        return clone;
    }
    public virtual void UseItem()
    {
        Debug.Log("Using item "+ Name +"Id "+ID);
        switch (ID)
        {
            case 1:
                // Ví dụ: ID 1 là item hồi máu
                PlayerController playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                {
                    playerController.ChangeHealth(10,DameType.TypeDamage.Heal); // 
                    RemoveFromStack(1);
                }
                break;
            default:
                break;
        }
    }
}
