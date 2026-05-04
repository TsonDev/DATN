using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] SoundData soundEff;
    public string ChestID { get; private set; }
    public bool IsOpened { get; private set; }
    public GameObject itemPrefabs;//item drop
    public Sprite opendedSprite;

    [Header("Behavior")]
    [Tooltip("Thời gian rương biến mất sau khi mở (0 = không bao giờ biến mất)")]
    [SerializeField] private float destroyAfterOpenedTime = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if(!CanInteract()) return;
        //Open chest
        OpenChest();
        if (soundEff != null)
        {
            SoundManager.Instance.PlaySound(soundEff);
        }
    }
    void OpenChest()
    {
        //SetOpened
        SetOpend(true);

        //Drop item
        if (itemPrefabs)
        {
            GameObject dropItem = Instantiate(itemPrefabs, transform.position + Vector3.down,Quaternion.identity);
            dropItem.GetComponent<BounceEffect>().StartBounce();
        }

        // Tự động huỷ sau 1 khoảng thời gian (nếu > 0)
        if (destroyAfterOpenedTime > 0f)
        {
            Destroy(gameObject, destroyAfterOpenedTime);
        }
    }
    public void SetOpend(bool open)
    {
        if(IsOpened = open)
        {
            GetComponent<SpriteRenderer>().sprite = opendedSprite;
        }
    }

}
