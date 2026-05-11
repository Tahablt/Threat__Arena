using UnityEngine;
using System.Collections.Generic;

public class PlayerSaveData : MonoBehaviour
{
    public List<string> itemIds = new List<string>(); // Oyuncunun aldýðý eþyalarýn ID'leri

    public void AddItem(string id)
    {
        itemIds.Add(id);
        Debug.Log("Envantere eklendi: " + id);
    }
}