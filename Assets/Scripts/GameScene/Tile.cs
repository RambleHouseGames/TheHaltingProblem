using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public int x;
    public int z;

    public virtual void DestroySelf()
    {
        Destroy(gameObject);
    }
}
