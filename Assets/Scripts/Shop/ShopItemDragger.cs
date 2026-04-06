using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemDragger : MonoBehaviour, IPointerClickHandler
{
    private bool isShopItem;
    public Slot originalIventorySlot;
    public void Initialize(bool shopItem)=>isShopItem = shopItem;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            if(isShopItem)
            {
                //buy
                BuyItem();
            }
            else
            {
                //sell
                SellItem();
            }
        }
    }
    private void BuyItem()
    {
        Item item = GetComponent<Item>();
        ShopSlot slot = GetComponentInParent<ShopSlot>();
        if(!item || !slot) return;
        if(CurrencyController.instance.GetGold() < slot.itemPrice)
        {
            return;
        }
        GameObject itemPrefab = FindAnyObjectByType<ItemDictionary>().GetItemPrefabs(item.ID);
        if(InventoryController.Intance.AddItem(itemPrefab))
        {
            CurrencyController.instance.SpendGold(slot.itemPrice);
            ShopController.instance.RefreshPlayerInventoryDisplay();
            ShopController.instance.RemoveItemFromShop(item.ID,1);
        }
        else
        {
            Debug.Log("Not enough space in inventory");
        }

    }
    private void SellItem()
    {
        Item item = GetComponent<Item>();
        ShopSlot slot = GetComponentInParent<ShopSlot>();
        if (!item || !slot|| !originalIventorySlot) return;
       Item invItem = originalIventorySlot.CurrentItem?.GetComponent<Item>();
        if(!invItem) return;
        if(invItem.quantity>1) invItem.RemoveFromStack(1);
        else
        {
            Destroy(originalIventorySlot.CurrentItem);
            originalIventorySlot.CurrentItem = null;
        }
        InventoryController.Intance.RebuildItemCounts();
        CurrencyController.instance.AddGold(slot.itemPrice);
        ShopController.instance.RefreshPlayerInventoryDisplay();
        //add item shop
        ShopController.instance.AddItemToShop(item.ID,1);

    }
}
