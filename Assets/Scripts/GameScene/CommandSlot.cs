using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandSlot : MonoBehaviour
{
    [SerializeField]
    private CommandSlot next;
    public CommandSlot Next { get { return next; } }

    
}
