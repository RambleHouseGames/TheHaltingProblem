using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSceneSignalManager : SignalManager
{
    public static MenuSceneSignalManager Inst;

    protected override void RegisterSingleton()
    {
        Inst = this;
    }
}
