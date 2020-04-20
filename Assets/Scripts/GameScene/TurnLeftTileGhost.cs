using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnLeftTileGhost : ItemGhost
{

    [SerializeField]
    private GameObject happyGhost;
    public override GameObject HappyGhost { get { return happyGhost; } }

    [SerializeField]
    private GameObject sadGhost;
    public override GameObject SadGhost { get { return sadGhost; } }

    [SerializeField]
    private Tile tilePrefab;
    public override Tile TilePrefab { get { return tilePrefab; } }

    public override bool CanBePlacedAt(int x, int z)
    {
        return TileGrid.Inst.GetTileAt(x, z) != null;
    }
}
