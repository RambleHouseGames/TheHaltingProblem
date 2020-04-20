using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSignalManager : SignalManager
{
    public static GameSceneSignalManager Inst;

    protected override void RegisterSingleton()
    {
        Inst = this;
    }
}
