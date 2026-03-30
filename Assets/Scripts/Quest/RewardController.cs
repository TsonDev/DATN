using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardController : MonoBehaviour
{
   public static RewardController instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void GiveQuestReward(Quest quest)
    {
        if (quest?.questRewards == null) return;
        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                   //Give reward item
                   GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Gold:
                    //Give reward item
                    break;
                case RewardType.Custom:
                    //Give reward item
                    break;
            }
        }

    }
    public void GiveItemReward(int itemID, int amout)
    {
        //Give reward item
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefabs(itemID);
        if(itemPrefab == null) return;
        for(int i = 0; i < amout; i++)
        {
            if (!InventoryController.Intance.AddItem(itemPrefab))
            {
                GameObject dropItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
                dropItem.GetComponent<BounceEffect>().StartBounce();
            }
            else
            {
                //showPopUp
                itemPrefab.GetComponent<Item>().ShowPopUp();
            }
        }
    }

}
