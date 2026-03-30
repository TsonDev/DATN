using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickupUIController : MonoBehaviour
{
    // Start is called before the first frame update
    public static ItemPickupUIController Instance { get; private set; }
    public GameObject popupItem;
    public int maxPopupItems = 5;
    public float popupDuration = 2f;

    private readonly Queue<GameObject> activePopups = new Queue<GameObject>();
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void ShowItemPickup(string itemName, Sprite iconItem)
    {
       GameObject newpopup = Instantiate(popupItem, transform);
        newpopup.GetComponentInChildren<TMP_Text>().text = itemName;
        Image popupImage = newpopup.transform.Find("ItemIcon").GetComponent<Image>();
        if (popupImage)
        {
            popupImage.sprite = iconItem;
        }
        activePopups.Enqueue(newpopup);
        if(activePopups.Count > maxPopupItems)
        {
            GameObject oldPopup = activePopups.Dequeue();
            Destroy(oldPopup);
        }
        //Fade out and destroy after duration
        StartCoroutine(FadeOutAndDestroy(newpopup));
    }
    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popupDuration);
        if(popup==null) yield break;
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        for(float timePassed = 0f; timePassed < 1f; timePassed += Time.deltaTime)
        {
            if(popup==null) yield break;
            canvasGroup.alpha =1f - timePassed;
            yield return null;
        }
        Destroy(popup);
    }
}
