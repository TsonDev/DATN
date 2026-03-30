using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    float minDropitem = 2f;
    float maxDropitem = 3f;

    private InventoryController inventoryController;
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Intance;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        Slot slotDrop = eventData.pointerEnter?.GetComponent<Slot>();
        if (slotDrop == null)
        {
            GameObject item = eventData.pointerEnter;
            if (item != null)
            {
                slotDrop = item.GetComponentInParent<Slot>();
            }
        }

        // origin slot (may be null if originalParent is not a player inventory Slot)
        Slot originalSlot = originalParent != null ? originalParent.GetComponent<Slot>() : null;
        // origin shop slot (if any)
        ShopSlot originalShopSlot = originalParent != null ? originalParent.GetComponentInParent<ShopSlot>() : null;
        bool originIsShop = originalShopSlot != null && originalShopSlot.isShopSlot;

        // If origin is a shop slot -> always revert to original position (no transfer)
        if (originIsShop)
        {
            RevertToOriginal();
            return;
        }

        // detect if target is hotbar
        var hotbarController = FindObjectOfType<HotBarController>();
        bool isTargetHotbar = false;
        if (hotbarController != null && slotDrop != null && hotbarController.HotBarPanel != null)
        {
            isTargetHotbar = slotDrop.transform.IsChildOf(hotbarController.HotBarPanel.transform);
        }

        // CASE: dropped onto a hotbar slot -> remove one from inventory and place to hotbar
        if (isTargetHotbar && slotDrop != null)
        {
            Item srcItem = GetComponent<Item>();
            if (srcItem == null)
            {
                RevertToOriginal();
                return;
            }

            // If origin is player inventory (originalSlot != null) then transfer 1 unit to hotbar
            if (originalSlot != null)
            {
                // If stack > 1: create small visual clone for hotbar and decrement original stack
                if (srcItem.quantity > 1)
                {
                    GameObject clone = srcItem.CloneItem(1);
                    clone.transform.SetParent(slotDrop.transform);
                    clone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slotDrop.CurrentItem = clone;

                    // remove one from original stack (updates UI via Item.RemoveFromStack)
                    srcItem.RemoveFromStack(1);

                    // if original stack became 0 (shouldn't if >1 branch), clean up, else revert UI back
                    if (srcItem.quantity <= 0)
                    {
                        originalSlot.CurrentItem = null;
                        Destroy(gameObject);
                    }
                    else
                    {
                        RevertToOriginal();
                    }

                    InventoryController.Intance?.RebuildItemCounts();
                    return;
                }
                else
                {
                    // quantity == 1 -> move the actual item UI to hotbar (no clone)
                    originalSlot.CurrentItem = null;
                    transform.SetParent(slotDrop.transform);
                    slotDrop.CurrentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    InventoryController.Intance?.RebuildItemCounts();
                    return;
                }
            }
            else
            {
                // origin is not player inventory (shouldn't happen here because shop was handled), revert
                RevertToOriginal();
                return;
            }
        }

        // CASE: dropped onto a normal inventory slot
        if (slotDrop != null)
        {
            Item dragerItem = GetComponent<Item>();
            if (slotDrop.CurrentItem != null)
            {
                Item targetItem = slotDrop.CurrentItem.GetComponent<Item>();
                if (dragerItem == null || targetItem == null)
                {
                    RevertToOriginal();
                    return;
                }

                if (targetItem.ID == dragerItem.ID)
                {
                    targetItem.AddtoStack(dragerItem.quantity);
                    if (originalSlot != null)
                    {
                        originalSlot.CurrentItem = null;
                        Destroy(gameObject);
                    }
                    else
                    {
                        // origin was not player inventory - revert the UI back (safety)
                        RevertToOriginal();
                    }
                }
                else
                {
                    // swap only if origin is an inventory slot (originalSlot != null)
                    if (originalSlot != null)
                    {
                        slotDrop.CurrentItem.transform.SetParent(originalParent.transform);
                        originalSlot.CurrentItem = slotDrop.CurrentItem;
                        slotDrop.CurrentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                        transform.SetParent(slotDrop.transform);
                        slotDrop.CurrentItem = gameObject;
                        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        RevertToOriginal();
                        Debug.LogWarning("OnEndDrag: cannot swap because original is not player inventory. Reverting.");
                    }
                }
            }
            else
            {
                // target slot empty -> move item (only if origin is player inventory)
                if (originalSlot != null)
                {
                    originalSlot.CurrentItem = null;
                    transform.SetParent(slotDrop.transform);
                    slotDrop.CurrentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
                else
                {
                    RevertToOriginal();
                }
            }
        }
        else
        {
            // Not dropped on any slot
            if (!isWithInInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                // inside inventory UI area but not on a slot -> return to original parent
                RevertToOriginal();
            }
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    void RevertToOriginal()
    {
        // return the UI element to its original parent and restore slot/shop references
        if (originalParent == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(originalParent);
        var rt = GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;

        // restore ShopSlot.currentItem if parent is in a ShopSlot
        var shopSlot = originalParent.GetComponentInParent<ShopSlot>();
        if (shopSlot != null)
        {
            shopSlot.currentItem = gameObject;
            return;
        }

        // otherwise restore Slot.CurrentItem
        var slot = originalParent.GetComponent<Slot>();
        if (slot != null)
        {
            slot.CurrentItem = gameObject;
        }
    }

    bool isWithInInventory(Vector2 mousePosition)
    {
        if (originalParent == null || originalParent.parent == null) return false;
        RectTransform rectTransform = originalParent.parent.GetComponent<RectTransform>();
        if (rectTransform == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition);
    }
    void DropItem(Slot originalSlot)
    {
        // Defensive: make sure we have an Item component
        Item item = GetComponent<Item>();
        if (item == null)
        {
            Debug.LogWarning("DropItem: dragged object has no Item component — destroying.");
            Destroy(gameObject);
            return;
        }

        int quatity = item.quantity;

        if (quatity > 1)
        {
            item.RemoveFromStack();
            if (originalParent != null) transform.SetParent(originalParent);
            var rt = GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = Vector2.zero;
            quatity = 1;
        }
        else
        {
            if (originalSlot != null)
            {
                originalSlot.CurrentItem = null;
            }
        }

        // Find player
        Transform playerTranform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTranform == null)
        {
            Debug.LogWarning("DropItem: Player transform not found.");
            return;
        }

        // Random position drop
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropitem, maxDropitem);
        Vector2 dropPosition = (Vector2)playerTranform.position + dropOffset;

        // Spawn item new (world pickup)
        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        if (droppedItem != null)
        {
            droppedItem.quantity = 1;
            var bounce = dropItem.GetComponent<BounceEffect>();
            if (bounce != null) bounce.StartBounce();
        }
        else
        {
            Debug.LogWarning("DropItem: spawned drop has no Item component.");
        }

        // Destroy UI object if we removed it from inventory
        if (quatity <= 1)
        {
            if (originalSlot == null || originalSlot.CurrentItem == null)
            {
                Destroy(gameObject);
            }
            else
            {
                // ensure UI is consistent
                RevertToOriginal();
            }
        }

        InventoryController.Intance?.RebuildItemCounts();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //split stack
            SplitStack();
            Debug.Log("Clicked right");
        }
    }
    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;
        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;
        item.RemoveFromStack(splitAmount);
        GameObject newItem = item.CloneItem(splitAmount);
        if (inventoryController == null && newItem == null) return;

        foreach(Transform slotTranform in inventoryController.InventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if(slot != null && slot.CurrentItem == null)
            {
                slot.CurrentItem = newItem;
                newItem.transform.SetParent(slotTranform.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }
        //no have slot return stack
        item.AddtoStack(splitAmount);
        Destroy(newItem);
    }

        
}
