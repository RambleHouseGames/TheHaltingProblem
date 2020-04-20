using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandSpawner : CommandSlot
{
    private bool gameIsOver = false;
    private bool lastCommandWasTurn = false;

    void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    private void onBeatStarted(Signal signal)
    {
        if (!gameIsOver)
        {
            BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
            if (beatStartedSignal.BeatType == BeatType.BOARD_MOVE)
            {
                if (lastCommandWasTurn)
                {
                    Command newCommand = CommandPool.Inst.GetCommand<GoStraightCommand>();
                    newCommand.Activate(this);
                    lastCommandWasTurn = false;
                }
                else
                {
                    Command newCommand = CommandPool.Inst.GetRandomCommand();
                    newCommand.Activate(this);
                    lastCommandWasTurn = (newCommand.GetType() != typeof(GoStraightCommand));
                }
            }
        }
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }
}
