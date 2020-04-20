using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command : MonoBehaviour
{
    private CommandState currentState;

    public CommandSlot currentSlot = null;

    private RectTransform rectTransform = null;
    public RectTransform RectTransform {
        get {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    void Update()
    {
        if (currentState != currentState.GetNextState())
        {
            currentState.End();
            currentState = currentState.GetNextState();
            currentState.Start();
        }
        currentState.Update();
    }

    public void Activate(CommandSlot startSlot)
    {
        currentSlot = startSlot;
        currentState = new CommandIdleState(this);
        currentState.Start();
    }

    public void Deactivate()
    {
        currentSlot = null;
    }
}

public abstract class CommandState
{
    protected Command command;

    public CommandState(Command command)
    {
        this.command = command;
    }

    public virtual void Start()
    {
        Debug.Log("Starting " + this.GetType().ToString());
    }

    public virtual void Update()
    {
        Debug.Log("Updating " + this.GetType().ToString());
    }

    public virtual void End()
    {
        Debug.Log("Ending " + this.GetType().ToString());
    }

    public abstract CommandState GetNextState();
}

public class CommandPoolState : CommandState
{
    private CommandState nextState;

    public CommandPoolState(Command command) : base(command)
    {
        nextState = this;
    }

    public override void Start() {
        CommandPool.Inst.ReturnToPool(command);
        command.transform.SetParent(CommandPool.Inst.transform);
        command.transform.localPosition = Vector3.zero;
    }
    public override void Update() {}
    public override void End() {}

    public override CommandState GetNextState()
    {
        return nextState;
    }
}

public class CommandIdleState : CommandState
{
    private CommandState nextState;
    private bool gameIsOver = false;

    public CommandIdleState(Command command) : base(command)
    {
        nextState = this;
    }

    public override void Start() {
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
        command.transform.SetParent(command.currentSlot.transform);
        command.RectTransform.localPosition = Vector3.zero;
    }
    public override void Update() {}
    public override void End() {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.RemoveListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    public override CommandState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        if(gameIsOver)
        {
            nextState = new CommandGameOverState(command);
            return;
        }

        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        if (beatStartedSignal.BeatType == BeatType.INTRO)
            return;

        if (beatStartedSignal.BeatType == BeatType.BOARD_MOVE)
        {
            if (command.currentSlot.Next != null)
                nextState = new CommandSlideState(command);
            else
                nextState = new CommandPoolState(command);
        }
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }
}

public class CommandSlideState : CommandState
{
    private CommandState nextState;
    private Vector3 startPosition;

    private bool gameIsOver = false;

    public CommandSlideState(Command command) : base(command)
    {
        nextState = this;
    }

    public override void Start()
    {
        if(command.currentSlot.Next != null)
        {
            command.currentSlot = command.currentSlot.Next;
            command.transform.SetParent(command.currentSlot.transform);
            startPosition = command.RectTransform.localPosition;
        }
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    public override void Update()
    {
        float totalDistance = Vector3.Distance(startPosition, Vector3.zero);
        float distaceFromStart = totalDistance * TempoManager.Inst.BeatPercentage;
        command.RectTransform.localPosition = Vector3.MoveTowards(startPosition, Vector3.zero, distaceFromStart);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.RemoveListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    public override CommandState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        if (gameIsOver)
        {
            nextState = new CommandGameOverState(command);
            return;
        }

        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.ROBOT_MOVE, "wrong BeatType started while command in Slide state.  Slide state expects to end on ROBOT_MOVE beat.");
        nextState = new CommandIdleState(command);
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }
}

public class CommandGameOverState : CommandState
{
    public CommandGameOverState(Command command) : base(command)
    { }

    public override void Start() {}

    public override void Update() {}

    public override void End() {}

    public override CommandState GetNextState()
    {
        return this;
    }
}