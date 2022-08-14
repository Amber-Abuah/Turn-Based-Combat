using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{
    [SerializeField] string itemName;
    [SerializeField] ItemType itemType;
    [SerializeField] int healAmount;

    public string ItemName { get { return itemName; } }
    public ItemType ItemType { get { return itemType; } }
    public int HealAmount { get { return healAmount; } }
}
