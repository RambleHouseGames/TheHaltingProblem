using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverSignal : Signal {}
public class BeatStartedSignal : Signal
{
    public BeatType BeatType;

    public BeatStartedSignal (BeatType beatType)
    {
        this.BeatType = beatType;
    }
}
public class RobotStartedFallingSignal : Signal { }
public class RobotFinishedFallingSignal : Signal { }
public class ItemIconClickedSignal : Signal
{
    public ItemIcon ItemIcon;

    public ItemIconClickedSignal(ItemIcon itemIcon)
    {
        this.ItemIcon = itemIcon;
    }
}
public class ItemLocationSelectedSignal : Signal {
    public ItemGhost ItemGhost;
    public int X;
    public int Z;

    public ItemLocationSelectedSignal(ItemGhost itemGhost, int x, int z)
    {
        this.ItemGhost = itemGhost;
        this.X = x;
        this.Z = z;
    }
}
