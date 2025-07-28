using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public int currentPlayerLevel;

    void Start()
    {
        currentPlayerLevel = 0;
    }

    public void AddLevel(int amount)
    {
        currentPlayerLevel += amount;
        Debug.Log("Player level is now: " + currentPlayerLevel);
    }
}
