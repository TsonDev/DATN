using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs ;
    private Dictionary<int, GameObject> itemDictionary;
    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i+1;
            }
        }
        foreach (var item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }
    public GameObject GetItemPrefabs(int id)
    {
        itemDictionary.TryGetValue(id, out var prefab);
        if( prefab == null)
        {
            Debug.LogWarning($"Item not have with id{id}");
        }
        return prefab;
    }
}
