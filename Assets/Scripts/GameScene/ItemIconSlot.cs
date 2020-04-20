using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIconSlot : MonoBehaviour
{
    [SerializeField]
    private ItemIconSlot next;
    public ItemIconSlot Next { get { return next; } }
}
