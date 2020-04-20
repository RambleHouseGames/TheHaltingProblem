using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalStateMachine : MonoBehaviour
{
    private GlobalState currentState;

    private void Awake()
    {
        currentState = new GlobalLoadState();
    }

    private void Start()
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
}

public abstract class GlobalState
{
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

    public abstract GlobalState GetNextState();
}

public class GlobalLoadState : GlobalState
{
    GlobalState nextState;

    public GlobalLoadState()
    {
        nextState = this;
    }

    public override void Start()
    {
        GlobalSignalManager.Inst.AddListenner<FinishedLoadingSignal>(onFinishedLoading);
    }

    public override void Update() {}

    public override void End()
    {
        GlobalSignalManager.Inst.RemoveListenner<FinishedLoadingSignal>(onFinishedLoading);
    }

    public override GlobalState GetNextState()
    {
        return nextState;
    }

    private void onFinishedLoading(Signal signal)
    {
        nextState = new GlobalMenuState();
    }
}

public class GlobalMenuState : GlobalState
{
    private GlobalState nextState;

    public GlobalMenuState()
    {
        this.nextState = this;
    }

    public override void Start()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        SceneManager.LoadSceneAsync("MenuScene", LoadSceneMode.Additive);
    }

    public override void Update() {}

    public override void End()
    {
        MenuSceneSignalManager.Inst.RemoveListenner<StartButtonPressedSignal>(onStartButtonPressed);
        SceneManager.UnloadSceneAsync("MenuScene");
    }

    public override GlobalState GetNextState()
    {
        return nextState;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Assert(scene.name == "MenuScene", "A scene other than MenuScene finished loading in Menu State.  This should never happen");
        MenuSceneSignalManager.Inst.AddListenner<StartButtonPressedSignal>(onStartButtonPressed);
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void onStartButtonPressed(Signal signal)
    {
        nextState = new GlobalGameState();
    }
}

public class GlobalGameState : GlobalState
{
    private GlobalState nextState;

    public GlobalGameState()
    {
        nextState = this;
    }

    public override void Start()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
    }

    public override void Update() {}

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<GameOverSignal>(onGameOver);
        SceneManager.UnloadSceneAsync("GameScene");
    }

    public override GlobalState GetNextState()
    {
        return nextState;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Assert(scene.name == "GameScene", "A scene other than GameScene finished loading in Game State.  This should never happen");
        GameSceneSignalManager.Inst.AddListenner<GameOverSignal>(onGameOver);
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void onGameOver(Signal signal)
    {
        nextState = new GlobalMenuState();
    }
}