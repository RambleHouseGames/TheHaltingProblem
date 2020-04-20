using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringBoardTile : Tile
{
    public bool shouldDestroy = false;
    private SpringBoardState currentState;

    private Animator myAnimator = null;
    public Animator MyAnimator
    {
        get
        {
            if (myAnimator == null)
                myAnimator = GetComponent<Animator>();
            return myAnimator;
        }
    }

    void Awake()
    {
        currentState = new SpringBoardIdleState(this);
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

public abstract class SpringBoardState
{
    protected SpringBoardTile springBoardTile;

    public SpringBoardState(SpringBoardTile springBoardTile)
    {
        this.springBoardTile = springBoardTile;
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

    public abstract SpringBoardState GetNextState();
}

public class SpringBoardIdleState : SpringBoardState
{
    SpringBoardState nextState;
    private float animationClipLength;

    public SpringBoardIdleState(SpringBoardTile springBoardTile) : base(springBoardTile)
    {
        nextState = this;
    }

    public override void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        springBoardTile.MyAnimator.Play("SpringBoardIdle", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
        if (springBoardTile.shouldDestroy)
            nextState = new SpringBoardDestroyState(springBoardTile);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override SpringBoardState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        if (beatStartedSignal.BeatType == BeatType.BOARD_MOVE)
            nextState = new SpringBoardSpringState(springBoardTile);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in springBoardTile.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "SpringBoardSpring")
                animationClipLength = animationClip.length;
        }
    }
}

public class SpringBoardSpringState : SpringBoardState
{
    SpringBoardState nextState;
    private Transform springBoard;
    private float animationClipLength;

    public SpringBoardSpringState(SpringBoardTile springBoardTile) : base(springBoardTile)
    {
        nextState = this;
    }

    public override void Start()
    {
        springBoardTile.MyAnimator.Play("SpringBoardSpring", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {

        if (springBoardTile.shouldDestroy)
            nextState = new SpringBoardDestroyState(springBoardTile);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override SpringBoardState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        nextState = new SpringBoardIdleState(springBoardTile);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in springBoardTile.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "SpringBoardSpring")
                animationClipLength = animationClip.length;
        }
    }
}

public class SpringBoardDestroyState : SpringBoardState
{
    public SpringBoardDestroyState(SpringBoardTile springBoardTile) : base(springBoardTile)
    {
    }

    public override void Start()
    {
        GameObject.Destroy(springBoardTile.gameObject);
    }

    public override void Update() { }
    public override void End() { }

    public override SpringBoardState GetNextState()
    {
        return this;
    }
}