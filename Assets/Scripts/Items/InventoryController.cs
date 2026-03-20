using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    public GameObject InventoryPanel;
    public GameObject slotprefab;
    public int slotCount;
    public GameObject[] listItemsPrefabs;

    public static InventoryController Intance {  get; private set; }
    Dictionary<int, int> itemsCountCache = new Dictionary<int, int>();
    public event Action OnInventoryChanged;//event dc goij từ bất cứ đâu khi có sự thay đổi trong inventory
    private void Awake()
    {
        if(Intance != null && Intance != this)
        {
            Destroy(gameObject);
            return;
        }
        Intance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
        RebuildItemCounts();
        //for (int i = 0; i < slotCount; i++)
        //{
        //    Slot slot = Instantiate(slotprefab,InventoryPanel.transform).GetComponent<Slot>();
        //    if(i< listItemsPrefabs.Length)
        //    {
        //        GameObject item = Instantiate(listItemsPrefabs[i],slot.transform);
        //        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //        slot.CurrentItem = item;
        //    }    
        //}
    }
    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();
        foreach(Transform slotTranform in InventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if(slot.CurrentItem!= null)
            {
                Item item = slot.CurrentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;
    public bool AddItem(GameObject itemPrefab)
    {
        Item itemAddto = itemPrefab.GetComponent<Item>();
        if (itemAddto == null) return false;
        //Add if it had
        foreach (Transform slotTranform in InventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.CurrentItem != null)
            {
                Item slotItem = slot.CurrentItem.GetComponent<Item>();
                if(slot.CurrentItem != null && slotItem.ID == itemAddto.ID)
                {
                    //Same item
                    slotItem.AddtoStack();
                    RebuildItemCounts();

                    return true;
                }
            }
        }
        //Add if it new
        foreach (Transform slotTranform in InventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.CurrentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.CurrentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }
        return false;
    }
    //public bool AddItem(GameObject itemPrefab)
    //{
    //    Item newItemData = itemPrefab.GetComponent<Item>();

    //    // tìm stack có sẵn
    //    foreach (Transform slotTransform in InventoryPanel.transform)
    //    {
    //        Slot slot = slotTransform.GetComponent<Slot>();

    //        if (slot.CurrentItem != null)
    //        {
    //            Item item = slot.CurrentItem.GetComponent<Item>();

    //            if (item.ID == newItemData.ID && item.quantity < item.maxStack)
    //            {
    //                item.quantity++;
    //                item.UpdateQuantity();
    //                return true;
    //            }
    //        }
    //    }

    //    // nếu không có stack thì tạo item mới
    //    foreach (Transform slotTransform in InventoryPanel.transform)
    //    {
    //        Slot slot = slotTransform.GetComponent<Slot>();

    //        if (slot.CurrentItem == null)
    //        {
    //            GameObject newItem = Instantiate(itemPrefab, slot.transform);
    //            newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

    //            Item item = newItem.GetComponent<Item>();
    //            item.quantity = 1;
    //            item.UpdateQuantity();

    //            slot.CurrentItem = newItem;

    //            return true;
    //        }
    //    }

    //    return false;
    //}
    //lưu vào file 
    public List<InvetorySaveData> GetInventoryItems()
    {
        List<InvetorySaveData> invetorySaveDatas = new List<InvetorySaveData>();
        foreach (Transform slotTranform in InventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot.CurrentItem != null) { 
                Item item = slot.CurrentItem.GetComponent<Item>();
                invetorySaveDatas.Add(new InvetorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTranform.GetSiblingIndex(),
                    quantity = item.quantity
                });
                
            }
        }
        return invetorySaveDatas;
    }
    //Lấy từ file save
    public void SetInventoryItems(List<InvetorySaveData> inventorySaveData) {
        //clear inventory panel
        foreach (Transform child in InventoryPanel.transform)
        {
            Destroy(child);
        }
        //Create slot
        for(int i = 0; i < slotCount; i++)
        {
            Instantiate(slotprefab, InventoryPanel.transform);
        }
        //populate slot with save
        foreach (InvetorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot =InventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemprefab = itemDictionary.GetItemPrefabs(data.itemID);
                if (itemprefab != null)
                {
                    GameObject item = Instantiate(itemprefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    Item itemComponet = item.GetComponent<Item>();
                    if (itemComponet != null && data.quantity>1)
                    {
                        itemComponet.quantity = data.quantity;
                        itemComponet.UpdateQuantity();
                    }
                    slot.CurrentItem = item;
                }
            }
        }
        RebuildItemCounts();
    }
}
