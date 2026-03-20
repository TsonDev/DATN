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
    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantity();
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
        cloneItem.quantity = quantity;
        cloneItem.UpdateQuantity();
        return clone;
    }
    public virtual void UseItem()
    {
        Debug.Log("Using item "+ Name +"Id "+ID);
    }
}
