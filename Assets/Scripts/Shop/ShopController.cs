using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController instance;
    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopIventoryGrid, playerInvetoryGrid;
    public GameObject ShopSlotPrefab;
    public TMP_Text playerMoneyText, shopTitleText;

    private ItemDictionary itemDictionary;
    private ShopNPC currentShop;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        shopPanel.SetActive(false);
        if(CurrencyController.instance != null)
        {
            CurrencyController.instance.OnGoldChanged +=UpdateMoneyDisplay ;
            UpdateMoneyDisplay(CurrencyController.instance.GetGold());
        }
    }
    private void UpdateMoneyDisplay(int amount)
    {
        if(playerMoneyText != null)
        {
            playerMoneyText.text = amount.ToString();
        }
    }
    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;
        shopPanel.SetActive(true);
        if(shopTitleText!= null)
            shopTitleText.text = shop.shopkeepername + "'s Shop";
        //Refresh shop inventory
        RefreshShopDisplay();
        //Refresh player inventory
        RefreshPlayerInventoryDisplay();
        Time.timeScale = 0f; // Pause the game

    }
    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game
    }
    public void RefreshShopDisplay()
    {
        if(currentShop == null)
        {
            Debug.LogWarning("No shop is currently open.");
            return;
        }
        //Clear current shop inventory display
        foreach (Transform child in shopIventoryGrid)
        {
            Destroy(child.gameObject);
        }
        //Display current shop inventory
        if (currentShop != null)
        {
            foreach (var stockItem in currentShop.GetCurrentStock())
            {
                if(stockItem.quantity<=0) continue; // Skip items that are out of stock
                //Create a new shop slot
                CreateShopSlot(shopIventoryGrid, stockItem.itemID, stockItem.quantity, true);

            }
        }
    }
    public void RefreshPlayerInventoryDisplay()
    {
        if(InventoryController.Intance == null)
        {
            Debug.LogWarning("No InventoryController found in the scene.");
            return;
        }
        //Clear current player inventory display
        foreach (Transform child in playerInvetoryGrid)
        {
            Destroy(child.gameObject);
        }
        //Display current player inventory
        foreach(Transform slotTranform  in InventoryController.Intance.InventoryPanel.transform)
        {
           Slot invetorySlot  =  slotTranform.GetComponent<Slot>();
            if (invetorySlot?.CurrentItem!=null)
            {
                Item originalItem = invetorySlot.CurrentItem.GetComponent<Item>();
                CreateShopSlot(playerInvetoryGrid, originalItem.ID, originalItem.quantity, false, invetorySlot);
            }

        }
    }
    private void CreateShopSlot(Transform grid, int itemID, int quantity, bool isShop, Slot original = null)
    {
      
        GameObject slotObj = Instantiate(ShopSlotPrefab, grid);
        GameObject itemPrefab = itemDictionary.GetItemPrefabs(itemID);
        if (itemPrefab == null) return;
        GameObject itemInstance = Instantiate(itemPrefab, slotObj.transform);
        itemInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Center the item in the slot
        Item item = itemInstance.GetComponent<Item>();
        item.quantity = quantity;
        item.UpdateQuantity();
        int price = isShop? item.buyPrice : item.GetSellPrice();
        ShopSlot shopSlot = slotObj.GetComponent<ShopSlot>();
        shopSlot.isShopSlot = isShop;
        shopSlot.SetItem(itemInstance, price);
        

        //ItemHandler
        ItemDragHandler itemDragHandler = itemInstance.GetComponent<ItemDragHandler>();
        if (itemDragHandler != null)
        {
            itemDragHandler.enabled = false;
        }
        ShopItemDragger shopHandler = itemInstance.AddComponent<ShopItemDragger>();
        shopHandler.Initialize(isShop);
        if(!isShop) shopHandler.originalIventorySlot = original;
    }
    public void AddItemToShop(int itemID, int quantity)
    {
        if (currentShop == null) return;
        currentShop.AddStock(itemID, quantity);
        RefreshShopDisplay();
    }
    public bool RemoveItemFromShop(int itemID, int quantity)
     {
            if (currentShop == null) return false;
           bool sucess =  currentShop.RemoveStock(itemID, quantity);
           if(sucess) RefreshShopDisplay();
           return sucess;
    }
}
