using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemGhost : MonoBehaviour
{
    public abstract GameObject HappyGhost { get; }
    public abstract GameObject SadGhost { get; }
    public abstract Tile TilePrefab { get; }

    public abstract bool CanBePlacedAt(int x, int z);
}
