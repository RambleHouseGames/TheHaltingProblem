using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public static TileGrid Inst;

    [SerializeField]
    private List<TilePrefabSet> prefabSets;

    [SerializeField]
    private int StartPlatformRadius = 4;

    private List<Tile> tiles = new List<Tile>();

    private void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        SpawnStartPlatform();
        GameSceneSignalManager.Inst.AddListenner<ItemLocationSelectedSignal>(onItemLocationSelected);
    }

    public Tile GetTileAt(int x, int z)
    {
        foreach(Tile tile in tiles)
        {
            if (tile.x == x && tile.z == z)
                return tile;
        }
        return null;
    }

    private void SpawnStartPlatform()
    {
        for(int x = -StartPlatformRadius; x <= StartPlatformRadius; x++)
        {
            for(int z = -StartPlatformRadius; z <= StartPlatformRadius; z++)
            {
                float distanceFromStart = Vector3.Distance(Vector3.zero, new Vector3((float)x, 0f, (float)z));
                if ((int)distanceFromStart <= StartPlatformRadius)
                    InstantiateTile<BasicTile>(x, z);
            }
        }
    }

    private void InstantiateTile<T>(int x, int z) where T : Tile
    {
        foreach (TilePrefabSet prefabSet in prefabSets)
        {
            if (prefabSet.prefab.GetType() == typeof(T))
            {
                Tile newTile = Instantiate<Tile>(prefabSet.prefab, new Vector3(x, 0, z), Quaternion.identity, transform);
                newTile.x = x;
                newTile.z = z;
                tiles.Add(newTile);
                return;
            }
        }
        Debug.Assert(false, "No PrefabSet found for Tile of Type: " + typeof(T).ToString());
    }

    private void onItemLocationSelected(Signal signal)
    {
        ItemLocationSelectedSignal itemLocationSelectedSignal = (ItemLocationSelectedSignal)signal;
        int x = itemLocationSelectedSignal.X;
        int z = itemLocationSelectedSignal.Z;
        Tile existingTile = GetTileAt(x, z);
        if (existingTile != null)
        {
            tiles.Remove(existingTile);
            existingTile.DestroySelf();
        }
        Tile newTile = Instantiate<Tile>(itemLocationSelectedSignal.ItemGhost.TilePrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
        newTile.x = itemLocationSelectedSignal.X;
        newTile.z = itemLocationSelectedSignal.Z;
        tiles.Add(newTile);
    }
}

[Serializable]
public class TilePrefabSet
{
    public Tile prefab;
}