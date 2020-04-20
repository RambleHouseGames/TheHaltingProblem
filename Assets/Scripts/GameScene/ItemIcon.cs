using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemIcon : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private ItemGhost itemGhost;
    public ItemGhost ItemGhost { get { return itemGhost; } }

    [SerializeField]
    private Tile tile;
    public Tile Tile { get { return tile; } }

    private ItemIconState currentState;

    [NonSerialized]
    public ItemIconSlot currentSlot = null;

    private RectTransform rectTransform = null;
    public RectTransform RectTransform
    {
        get
        {
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

    public void Activate(ItemIconSlot startSlot)
    {
        currentSlot = startSlot;
        currentState = new ItemIconIdleState(this);
        currentState.Start();
    }

    public void Deactivate()
    {
        currentSlot = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!PlacementHandler.Inst.PlacementIsInProgress)
        {
            ItemIconClickedSignal itemIconClickedSignal = new ItemIconClickedSignal(this);
            GameSceneSignalManager.Inst.FireSignal(itemIconClickedSignal);
        }
    }
}

public abstract class ItemIconState
{
    protected ItemIcon itemIcon;

    public ItemIconState(ItemIcon itemIcon)
    {
        this.itemIcon = itemIcon;
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

    public abstract ItemIconState GetNextState();
}

public class ItemIconPoolState : ItemIconState
{
    private ItemIconState nextState;

    public ItemIconPoolState(ItemIcon itemIcon) : base(itemIcon)
    {
        nextState = this;
    }

    public override void Start()
    {
        ItemIconPool.Inst.ReturnToPool(itemIcon);
        itemIcon.transform.SetParent(CommandPool.Inst.transform);
        itemIcon.transform.localPosition = Vector3.zero;
    }
    public override void Update() { }
    public override void End() { }

    public override ItemIconState GetNextState()
    {
        return nextState;
    }
}

public class ItemIconIdleState : ItemIconState
{
    private ItemIconState nextState;
    private bool gameIsOver = false;
    private bool gotClicked = false;

    public ItemIconIdleState(ItemIcon itemIcon) : base(itemIcon)
    {
        nextState = this;
    }

    public override void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
        GameSceneSignalManager.Inst.AddListenner<ItemIconClickedSignal>(onItemIconClicked);
        itemIcon.transform.SetParent(itemIcon.currentSlot.transform);
        itemIcon.RectTransform.localPosition = Vector3.zero;
    }
    public override void Update() { }
    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.RemoveListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
        GameSceneSignalManager.Inst.RemoveListenner<ItemIconClickedSignal>(onItemIconClicked);
    }

    public override ItemIconState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        if (gameIsOver)
        {
            nextState = new ItemIconGameOverState(itemIcon);
            return;
        }

        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        if (beatStartedSignal.BeatType == BeatType.INTRO)
            return;

        if (beatStartedSignal.BeatType == BeatType.BOARD_MOVE)
        {
            if (itemIcon.currentSlot.Next != null)
                nextState = new ItemIconSlideState(itemIcon);
            else
                nextState = new ItemIconPoolState(itemIcon);
        }
    }

    private void onItemIconClicked(Signal signal)
    {
        ItemIconClickedSignal itemIconClickedSignal = (ItemIconClickedSignal)signal;
        if(itemIconClickedSignal.ItemIcon == itemIcon)
        {
            itemIcon.transform.position = ItemIconPool.Inst.transform.position;
            gotClicked = true;
            nextState = new ItemIconPoolState(itemIcon);
        }
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }
}

public class ItemIconSlideState : ItemIconState
{
    private ItemIconState nextState;
    private Vector3 startPosition;
    private bool gotClicked = false;

    private bool gameIsOver = false;

    public ItemIconSlideState(ItemIcon itemIcon) : base(itemIcon)
    {
        nextState = this;
    }

    public override void Start()
    {
        if (itemIcon.currentSlot.Next != null)
        {
            itemIcon.currentSlot = itemIcon.currentSlot.Next;
            itemIcon.transform.SetParent(itemIcon.currentSlot.transform);
            startPosition = itemIcon.RectTransform.localPosition;
        }
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
        GameSceneSignalManager.Inst.AddListenner<ItemIconClickedSignal>(onItemIconClicked);
    }

    public override void Update()
    {
        if (!gotClicked)
        {
            float totalDistance = Vector3.Distance(startPosition, Vector3.zero);
            float distaceFromStart = totalDistance * TempoManager.Inst.BeatPercentage;
            itemIcon.RectTransform.localPosition = Vector3.MoveTowards(startPosition, Vector3.zero, distaceFromStart);
        }
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.RemoveListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    public override ItemIconState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        if (gameIsOver)
        {
            nextState = new ItemIconGameOverState(itemIcon);
            return;
        }

        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.ROBOT_MOVE, "wrong BeatType started while command in Slide state.  Slide state expects to end on ROBOT_MOVE beat.");
        nextState = new ItemIconIdleState(itemIcon);
    }

    private void onItemIconClicked(Signal signal)
    {
        ItemIconClickedSignal itemIconClickedSignal = (ItemIconClickedSignal)signal;
        if (itemIconClickedSignal.ItemIcon == itemIcon)
        {
            itemIcon.transform.position = ItemIconPool.Inst.transform.position;
            gotClicked = true;
            nextState = new ItemIconPoolState(itemIcon);
        }
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }
}

public class ItemIconGameOverState : ItemIconState
{
    public ItemIconGameOverState(ItemIcon itemIcon) : base(itemIcon)
    { }

    public override void Start() { }

    public override void Update() { }

    public override void End() { }

    public override ItemIconState GetNextState()
    {
        return this;
    }
}
