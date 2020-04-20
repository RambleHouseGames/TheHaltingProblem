using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSignalManager : SignalManager
{
    public static GlobalSignalManager Inst;

    protected override void RegisterSingleton()
    {
        Inst = this;
    }
}
