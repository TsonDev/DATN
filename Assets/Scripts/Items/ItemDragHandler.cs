using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler,IDragHandler, IPointerClickHandler
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
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot slotDrop = eventData.pointerEnter?.GetComponent<Slot>();
        if (slotDrop == null)
        {
            GameObject item = eventData.pointerEnter;
            if (item != null)
            {
                slotDrop = item.GetComponentInParent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();
        if (originalSlot != null)
        {
            originalSlot.CurrentItem = null;
        }
        if (slotDrop != null)
        {
            if (slotDrop.CurrentItem != null)
            {
                //drop item stack
                Item dragerItem = GetComponent<Item>();
                Item targetItem = slotDrop.CurrentItem.GetComponent<Item>();
                if (targetItem.ID == dragerItem.ID)
                {
                    targetItem.AddtoStack(dragerItem.quantity);
                    originalSlot.CurrentItem = null;
                    Destroy(gameObject);

                }
                else
                {
                    //swap items
                    slotDrop.CurrentItem.transform.SetParent(originalParent.transform);
                    originalSlot.CurrentItem = slotDrop.CurrentItem;
                    slotDrop.CurrentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    transform.SetParent(slotDrop.transform);
                    slotDrop.CurrentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
                  
            }
            else
            {
                originalSlot.CurrentItem = null;
                transform.SetParent(slotDrop.transform);
                slotDrop.CurrentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            //Move item  drop 
            //transform.SetParent(slotDrop.transform);
            //slotDrop.CurrentItem = gameObject;
        }
        else
        {
            //Nếu nó không nằm trong inventory
            if (!isWithInInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {     // quay về ô ban đầu
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
           
        }
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        
    }
    bool isWithInInventory(Vector2 mousePosition)
    {
        RectTransform rectTransform = originalParent.parent.GetComponent<RectTransform>(); 
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition);
    }
    void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        int quatity = item.quantity;
        if (quatity > 1)
        {
            item.RemoveFromStack();
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            quatity = 1;
        }
        else
        {
            originalSlot.CurrentItem = null;
        }
        originalSlot.CurrentItem = null;
        //Find player
        Transform playerTranform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if(playerTranform == null)
        {
            Debug.Log("Miss player tranform");
            return;
        }
        //Random position drop
        Vector2 dropOffset = Random.insideUnitCircle.normalized*Random.Range(minDropitem, maxDropitem);
        Vector2 dropPosition = (Vector2)playerTranform.position + dropOffset;
        //Spawn item new
       GameObject dropItem =  Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        droppedItem.quantity = 1;
        dropItem.GetComponent<BounceEffect>().StartBounce();
        //Destroy item in inventory
        if(quatity<=1 && originalSlot.CurrentItem == null)
        {
            Destroy(gameObject);
        }

        InventoryController.Intance.RebuildItemCounts();
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
