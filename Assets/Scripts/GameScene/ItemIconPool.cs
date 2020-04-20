using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIconPool : MonoBehaviour
{
    public static ItemIconPool Inst;

    [SerializeField]
    private List<ItemIconPrefab> itemIconPrefabs;

    private List<ItemIcon> activeItemIcons = new List<ItemIcon>();
    private List<ItemIcon> innactiveItemIcons = new List<ItemIcon>();

    void Awake()
    {
        Inst = this;

        foreach (ItemIconPrefab itemIconPrefab in itemIconPrefabs)
        {
            for (int i = 0; i < itemIconPrefab.StartingPool; i++)
            {
                innactiveItemIcons.Add(Instantiate<ItemIcon>(itemIconPrefab.Prefab, Vector3.zero, Quaternion.identity, transform));
            }
        }
    }

    public ItemIcon GetRandomItemIcon()
    {
        int sumBias = 0;
        foreach (ItemIconPrefab itemIconPrefab in itemIconPrefabs)
        {
            sumBias += itemIconPrefab.FrequencyBias;
        }
        int random = UnityEngine.Random.Range(0, sumBias);
        foreach (ItemIconPrefab itemIconPrefab in itemIconPrefabs)
        {
            if (random < itemIconPrefab.FrequencyBias)
            {
                return GetItemIcon(itemIconPrefab);
            }
            random -= itemIconPrefab.FrequencyBias;
        }
        Debug.Assert(false, "Random seems to be Greater Than or Equal To sumBias.  This shouldn't be possible.");
        return null;
    }

    public T GetItemIcon<T>() where T : ItemIcon
    {
        foreach (ItemIcon itemIcon in innactiveItemIcons)
        {
            if (itemIcon.GetType() == typeof(T))
            {
                unpool(itemIcon);
                return (T)itemIcon;
            }
        }
        T newItemIcon = InstantiateItemIcon<T>();
        activeItemIcons.Add(newItemIcon);
        return newItemIcon;
    }

    public ItemIcon GetItemIcon(ItemIconPrefab itemIconPrefab)
    {
        foreach (ItemIcon itemIcon in innactiveItemIcons)
        {
            if (itemIcon.GetType() == itemIconPrefab.Prefab.GetType())
            {
                unpool(itemIcon);
                return itemIcon;
            }
        }
        ItemIcon newItemIcon = InstantiateItemIcon(itemIconPrefab);
        activeItemIcons.Add(newItemIcon);
        return newItemIcon;
    }

    public void ReturnToPool(ItemIcon itemIcon)
    {
        itemIcon.Deactivate();
        pool(itemIcon);
    }

    private T InstantiateItemIcon<T>() where T : ItemIcon
    {
        foreach (ItemIconPrefab itemIconPrefab in itemIconPrefabs)
        {
            if (itemIconPrefab.Prefab.GetType() == typeof(T))
                return (T)Instantiate<ItemIcon>(itemIconPrefab.Prefab, Vector3.zero, Quaternion.identity, transform);
        }
        Debug.Assert(false, "No ItemIconPrefab found with type: " + typeof(T).ToString());
        return null;
    }

    private ItemIcon InstantiateItemIcon(ItemIconPrefab itemIconPrefab)
    {
        return Instantiate<ItemIcon>(itemIconPrefab.Prefab, Vector3.zero, Quaternion.identity, transform);
    }

    private void pool(ItemIcon itemIcon)
    {
        activeItemIcons.Remove(itemIcon);
        innactiveItemIcons.Add(itemIcon);
    }

    private void unpool(ItemIcon itemIcon)
    {
        innactiveItemIcons.Remove(itemIcon);
        activeItemIcons.Add(itemIcon);
    }
}

[Serializable]
public class ItemIconPrefab
{
    public ItemIcon Prefab;
    public int FrequencyBias;
    public int StartingPool;
}
