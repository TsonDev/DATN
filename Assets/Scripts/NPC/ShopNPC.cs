using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour,IInteractable
{
    public string ShopID = "shop_messi_1";
    public string shopkeepername = "Messi";
    public List<ShopstockItem> defaultShopStock = new List<ShopstockItem>();
    private List<ShopstockItem> currentShopStock = new List<ShopstockItem>();

    private bool isInitialized = false;// Flag to check if the shop has been initialized
    [System.Serializable]
    public class ShopstockItem
    {
        public int itemID;
        public int quantity;
    }

    void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        if (isInitialized) return;
        //Default stock items should be set in the inspector for each shop NPC
        currentShopStock = new List<ShopstockItem>();
        foreach (var item in defaultShopStock)
        {
            currentShopStock.Add(new ShopstockItem { itemID = item.itemID, quantity = item.quantity });
        }
        isInitialized = true;
    }
    public bool CanInteract()
    {
        //Shop only open in day or night
        //return Timemanger.isDay()
        //return questManager.IsQuestCompleted("some_quest_id");
        return true;
    }

    public void Interact()
    {
        if (ShopController.instance == null)
            return;
        if(ShopController.instance.shopPanel.activeSelf)
        {
            ShopController.instance.CloseShop();
        }
        else
        {
            ShopController.instance.OpenShop(this);
        }
    }
    public List<ShopstockItem> GetCurrentStock()
    {
        return currentShopStock;
    }
    public void SetStock(List<ShopstockItem> newStock)
    {
        currentShopStock = newStock;
    }
    public void AddStock(int quantity, int itemID)
    {
        var existingItem = currentShopStock.Find(s => s.itemID == itemID);
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            currentShopStock.Add(new ShopstockItem { itemID = itemID, quantity = quantity });
        }
    }
    public bool RemoveStock(int quantity, int itemID)
    {
        var existingItem = currentShopStock.Find(s => s.itemID == itemID);
        if (existingItem != null && existingItem.quantity>= quantity)
        {
            existingItem.quantity -= quantity;
            return true;
        }
        return false;
    }



}
