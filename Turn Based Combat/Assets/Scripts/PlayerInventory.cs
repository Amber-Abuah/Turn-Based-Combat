using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] Item item1;
    [SerializeField] Item item2;
    Dictionary<Item, int> items;

    public static PlayerInventory Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        items = new Dictionary<Item, int>();
        items.Add(item1, 4);
        items.Add(item2, 1);
    }

    public List<Item> GetItems()
    {
        return new List<Item>(items.Keys);
    }

    public void UseItem(Item item)
    {
        items[item] -= 1;

        if (items[item] == 0)
            items.Remove(item);
    }

    public int GetItemQuantity(Item item)
    {
        return items[item];
    }
}
