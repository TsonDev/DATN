using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyController : MonoBehaviour
{
    public static CurrencyController instance;
    [SerializeField] private int startGold = 100;
    private int playerGold =100;
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if(instance!= null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            playerGold = startGold;
        }
    }
    public int GetGold()
    {
        return playerGold;
    }
    public bool SpendGold(int amount)
    {
        if (amount <= playerGold)
        {
            playerGold -= amount;
            OnGoldChanged?.Invoke(playerGold);
            return true;
        }
        else
        {
            return false;
        }
    }
    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGoldChanged?.Invoke(playerGold);
    }
    public void SetGold(int amount)
    {
        playerGold = amount;
        OnGoldChanged?.Invoke(playerGold);
    }
}
