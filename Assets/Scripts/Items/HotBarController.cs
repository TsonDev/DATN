using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotBarController : MonoBehaviour
{
    public GameObject HotBarPanel;
    public GameObject slotPrefab;
    public int slotCount = 8;

    private ItemDictionary itemDictionary;
    private Key[] hotBarKeys;
    private void Awake()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        hotBarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotBarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check pressed
        for(int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotBarKeys[i]].wasPressedThisFrame) {
                //use item in slot
                UseItemInSlot(i);
            }
        }
    }
    public void UseItemInSlot(int index)
    {
        Slot slot = HotBarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.CurrentItem != null)
        {
            //UseItem
            slot.CurrentItem.GetComponent<Item>().UseItem();
            Destroy(slot.CurrentItem);
            slot.CurrentItem = null;
        }
    }
    public List<InvetorySaveData> GetBarItems()
    {
        List<InvetorySaveData> hotBarSaveDatas = new List<InvetorySaveData>();
        foreach (Transform slotTranform in HotBarPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot.CurrentItem != null)
            {
                Item item = slot.CurrentItem.GetComponent<Item>();
                hotBarSaveDatas.Add(new InvetorySaveData { itemID = item.ID, 
                    slotIndex = slotTranform.GetSiblingIndex() ,
                    //quantity = item.quantity,
                
                });
            }
        }
        return hotBarSaveDatas;
    }
    //Lấy từ file save
    public void SetHotBarItems(List<InvetorySaveData> hotBarSaveDatas)
    {
        //clear inventory panel
        foreach (Transform child in HotBarPanel.transform)
        {
            Destroy(child);
        }
        //Create slot
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, HotBarPanel.transform);
        }
        //populate slot with save
        foreach (InvetorySaveData data in hotBarSaveDatas)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = HotBarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemprefab = itemDictionary.GetItemPrefabs(data.itemID);
                if (itemprefab != null)
                {
                    GameObject item = Instantiate(itemprefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    //Item itemScript = item.GetComponent<Item>();
                    //itemScript.quantity = data.quantity;
                    //itemScript.UpdateQuantity();

                    slot.CurrentItem = item;
                }
            }
        }
    }
}
