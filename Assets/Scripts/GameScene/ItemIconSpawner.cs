using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIconSpawner : ItemIconSlot
{
    private bool gameIsOver = false;

    void Start()
    {
        //ItemIcon startItemIcon = ItemIconPool.Inst.GetRandomItemIcon();
        //startItemIcon.Activate(this);
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    private void onBeatStarted(Signal signal)
    {
        if (!gameIsOver)
        {
            BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
            if (beatStartedSignal.BeatType == BeatType.ROBOT_MOVE)
            {
                ItemIcon newItemIcon = ItemIconPool.Inst.GetRandomItemIcon();
                newItemIcon.Activate(this);
            }
        }
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }
}
