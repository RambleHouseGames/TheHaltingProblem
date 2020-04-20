using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnLeftTile : Tile
{
    public bool shouldDestroy = false;
    private TurnLeftTileState currentState;

    void Awake()
    {
        currentState = new TurnLeftTileIdleState(this);
    }

    void Start()
    {
        currentState.Start();
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

    public override void DestroySelf()
    {
        shouldDestroy = true;
    }
}

public abstract class TurnLeftTileState
{
    protected TurnLeftTile turnLeftTile;

    public TurnLeftTileState(TurnLeftTile turnLeftTile)
    {
        this.turnLeftTile = turnLeftTile;
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

    public abstract TurnLeftTileState GetNextState();
}

public class TurnLeftTileIdleState : TurnLeftTileState
{
    TurnLeftTileState nextState;

    public TurnLeftTileIdleState(TurnLeftTile turnLeftTile) : base(turnLeftTile)
    {
        nextState = this;
    }

    public override void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override void Update()
    {
        if (turnLeftTile.shouldDestroy)
            nextState = new TurnLeftTileDestroyState(turnLeftTile);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override TurnLeftTileState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        if (beatStartedSignal.BeatType == BeatType.BOARD_MOVE)
            nextState = new TurnLeftTileSpinState(turnLeftTile);
    }
}

public class TurnLeftTileSpinState : TurnLeftTileState
{
    TurnLeftTileState nextState;
    private Transform turnTable;
    private float startRotation;

    public TurnLeftTileSpinState(TurnLeftTile turnLeftTile) : base(turnLeftTile)
    {
        nextState = this;
    }

    public override void Start()
    {
        turnTable = turnLeftTile.transform.GetChild(0);
        startRotation = turnTable.rotation.eulerAngles.y;
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override void Update()
    {
        float rotation = TempoManager.Inst.BeatPercentage * 90f;
        float totalRotation = startRotation - rotation;
        turnTable.rotation = Quaternion.Euler(0f, totalRotation, 0f);
        if (turnLeftTile.shouldDestroy)
            nextState = new TurnLeftTileDestroyState(turnLeftTile);
    }

    public override void End()
    {
        turnTable.rotation = Quaternion.Euler(0f, startRotation - 90f, 0f);
    }

    public override TurnLeftTileState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        nextState = new TurnLeftTileIdleState(turnLeftTile);
    }
}

public class TurnLeftTileDestroyState : TurnLeftTileState
{
    public TurnLeftTileDestroyState(TurnLeftTile turnLeftTile) : base(turnLeftTile)
    {
    }

    public override void Start()
    {
        GameObject.Destroy(turnLeftTile.gameObject);
    }

    public override void Update() { }
    public override void End() {}

    public override TurnLeftTileState GetNextState()
    {
        return this;
    }
}