using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public static Robot Inst;

    private RobotState currentState;

    private Animator myAnimator = null;
    public Animator MyAnimator {
        get {
            if (myAnimator == null)
                myAnimator = GetComponent<Animator>();
            return myAnimator;
        }
    }

    public int currentX = 0;
    public int currentZ = 0;

    public Heading currentHeading = Heading.NORTH;

    void Awake()
    {
        Inst = this;
        currentState = new RobotIntroState(this);
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
}

public abstract class RobotState
{
    protected Robot robot;

    public RobotState(Robot robot)
    {
        this.robot = robot;
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

    public abstract RobotState GetNextState();
}

public class RobotIntroState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotIntroState(Robot robot) : base(robot)
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
        robot.MyAnimator.Play("Idle", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        if (beatStartedSignal.BeatType == BeatType.BOARD_MOVE)
            nextState = new RobotIdleState(robot);
        else if(beatStartedSignal.BeatType == BeatType.ROBOT_MOVE)
        {
            Command frontCommand = CommandRegister.Inst.GetCommand();
            if (frontCommand.GetType() == typeof(GoStraightCommand))
                nextState = new RobotWalkForwardState(robot);
            else if (frontCommand.GetType() == typeof(TurnLeftCommand))
                nextState = new RobotTurnLeftState(robot);
            else if (frontCommand.GetType() == typeof(TurnRightCommand))
                nextState = new RobotTurnRightState(robot);
        }
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "Idle")
                animationClipLength = animationClip.length;
        }
    }
}

public class RobotIdleState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotIdleState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        snapRotationToHeading(robot.currentHeading);
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("Idle", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.ROBOT_MOVE, "Wrong BeatType during robot idle state.  Idle State expects to be ended by ROBOT_MOVE beat");
        Command frontCommand = CommandRegister.Inst.GetCommand();
        if (frontCommand.GetType() == typeof(GoStraightCommand))
            nextState = new RobotWalkForwardState(robot);
        else if (frontCommand.GetType() == typeof(TurnLeftCommand))
            nextState = new RobotTurnLeftState(robot);
        else if (frontCommand.GetType() == typeof(TurnRightCommand))
            nextState = new RobotTurnRightState(robot);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "Idle")
                animationClipLength = animationClip.length;
        }
    }

    private void snapRotationToHeading(Heading heading)
    {
        switch(heading)
        {
            case Heading.NORTH:
                robot.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case Heading.EAST:
                robot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case Heading.SOUTH:
                robot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case Heading.WEST:
                robot.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
    }
}

public class RobotWalkForwardState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotWalkForwardState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        snapRotationToHeading(robot.currentHeading);

        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("WalkForward", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.BOARD_MOVE, "Wrong BeatType during robot walk forward state.  Walk forward State expects to be ended by BOARD_MOVE beat");
        switch (robot.currentHeading)
        {
            case Heading.NORTH:
                robot.currentZ++;
                break;
            case Heading.EAST:
                robot.currentX++;
                break;
            case Heading.SOUTH:
                robot.currentZ--;
                break;
            case Heading.WEST:
                robot.currentX--;
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
        Tile newTile = TileGrid.Inst.GetTileAt(robot.currentX, robot.currentZ);
        if (newTile == null)
            nextState = new RobotFallState(robot);
        else if (newTile.GetType() == typeof(TurnLeftTile))
            nextState = new RobotLeftTurnTableState(robot);
        else if (newTile.GetType() == typeof(SpringBoardTile))
            nextState = new RobotSpringState(robot);
        else
            nextState = new RobotIdleState(robot);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "WalkForward")
                animationClipLength = animationClip.length;
        }
    }

    private void snapRotationToHeading(Heading heading)
    {
        switch (heading)
        {
            case Heading.NORTH:
                robot.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case Heading.EAST:
                robot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case Heading.SOUTH:
                robot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case Heading.WEST:
                robot.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
    }
}

public class RobotTurnLeftState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotTurnLeftState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        snapRotationToHeading(robot.currentHeading);

        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("TurnLeft", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.BOARD_MOVE, "Wrong BeatType during robot turn left state.  Turn left State expects to be ended by BOARD_MOVE beat");
        switch (robot.currentHeading)
        {
            case Heading.NORTH:
                robot.currentHeading = Heading.WEST;
                break;
            case Heading.EAST:
                robot.currentHeading = Heading.NORTH;
                break;
            case Heading.SOUTH:
                robot.currentHeading = Heading.EAST;
                break;
            case Heading.WEST:
                robot.currentHeading = Heading.SOUTH;
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
        Tile newTile = TileGrid.Inst.GetTileAt(robot.currentX, robot.currentZ);
        if (newTile.GetType() == typeof(TurnLeftTile))
            nextState = new RobotLeftTurnTableState(robot);
        else
            nextState = new RobotIdleState(robot);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "TurnLeft")
                animationClipLength = animationClip.length;
        }
    }

    private void snapRotationToHeading(Heading heading)
    {
        switch (heading)
        {
            case Heading.NORTH:
                robot.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case Heading.EAST:
                robot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case Heading.SOUTH:
                robot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case Heading.WEST:
                robot.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
    }
}

public class RobotTurnRightState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotTurnRightState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        snapRotationToHeading(robot.currentHeading);

        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("TurnRight", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.BOARD_MOVE, "Wrong BeatType during robot turn left state.  Turn left State expects to be ended by BOARD_MOVE beat");
        switch (robot.currentHeading)
        {
            case Heading.NORTH:
                robot.currentHeading = Heading.EAST;
                break;
            case Heading.EAST:
                robot.currentHeading = Heading.SOUTH;
                break;
            case Heading.SOUTH:
                robot.currentHeading = Heading.WEST;
                break;
            case Heading.WEST:
                robot.currentHeading = Heading.NORTH;
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
        Tile newTile = TileGrid.Inst.GetTileAt(robot.currentX, robot.currentZ);
        if (newTile.GetType() == typeof(TurnLeftTile))
            nextState = new RobotLeftTurnTableState(robot);
        else
            nextState = new RobotIdleState(robot);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "TurnRight")
                animationClipLength = animationClip.length;
        }
    }

    private void snapRotationToHeading(Heading heading)
    {
        switch (heading)
        {
            case Heading.NORTH:
                robot.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case Heading.EAST:
                robot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case Heading.SOUTH:
                robot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case Heading.WEST:
                robot.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
    }
}

public class RobotLeftTurnTableState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotLeftTurnTableState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        snapRotationToHeading(robot.currentHeading);

        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("LeftTurnTable", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.ROBOT_MOVE, "Wrong BeatType during robot left trn table state.  Left turn table State expects to be ended by ROBOT_MOVE beat");
        switch (robot.currentHeading)
        {
            case Heading.NORTH:
                robot.currentHeading = Heading.WEST;
                break;
            case Heading.EAST:
                robot.currentHeading = Heading.NORTH;
                break;
            case Heading.SOUTH:
                robot.currentHeading = Heading.EAST;
                break;
            case Heading.WEST:
                robot.currentHeading = Heading.SOUTH;
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }

        Command frontCommand = CommandRegister.Inst.GetCommand();
        if (frontCommand.GetType() == typeof(GoStraightCommand))
            nextState = new RobotWalkForwardState(robot);
        else if (frontCommand.GetType() == typeof(TurnLeftCommand))
            nextState = new RobotTurnLeftState(robot);
        else if (frontCommand.GetType() == typeof(TurnRightCommand))
            nextState = new RobotTurnRightState(robot);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "LeftTurnTable")
                animationClipLength = animationClip.length;
        }
    }

    private void snapRotationToHeading(Heading heading)
    {
        switch (heading)
        {
            case Heading.NORTH:
                robot.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case Heading.EAST:
                robot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case Heading.SOUTH:
                robot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case Heading.WEST:
                robot.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
    }
}

public class RobotSpringState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotSpringState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        snapRotationToHeading(robot.currentHeading);

        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("Spring", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        BeatStartedSignal beatStartedSignal = (BeatStartedSignal)signal;
        Debug.Assert(beatStartedSignal.BeatType == BeatType.ROBOT_MOVE, "Wrong BeatType during robot left trn table state.  Left turn table State expects to be ended by ROBOT_MOVE beat");

        switch (robot.currentHeading)
        {
            case Heading.NORTH:
                robot.currentZ += 2;
                break;
            case Heading.EAST:
                robot.currentX += 2;
                break;
            case Heading.SOUTH:
                robot.currentZ -= 2;
                break;
            case Heading.WEST:
                robot.currentX -= 2;
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }

        Tile newTile = TileGrid.Inst.GetTileAt(robot.currentX, robot.currentZ);
        if (newTile == null)
            nextState = new RobotFallState(robot);
        else
        {
            Command frontCommand = CommandRegister.Inst.GetCommand();
            if (frontCommand.GetType() == typeof(GoStraightCommand))
                nextState = new RobotWalkForwardState(robot);
            else if (frontCommand.GetType() == typeof(TurnLeftCommand))
                nextState = new RobotTurnLeftState(robot);
            else if (frontCommand.GetType() == typeof(TurnRightCommand))
                nextState = new RobotTurnRightState(robot);
        }
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "Spring")
                animationClipLength = animationClip.length;
        }
    }

    private void snapRotationToHeading(Heading heading)
    {
        switch (heading)
        {
            case Heading.NORTH:
                robot.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case Heading.EAST:
                robot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case Heading.SOUTH:
                robot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case Heading.WEST:
                robot.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            default:
                Debug.Assert(false, "Unknown heading: " + robot.currentHeading.ToString());
                break;
        }
    }
}

public class RobotFallState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotFallState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        RobotStartedFallingSignal robotStartedFallingSignal = new RobotStartedFallingSignal();
        GameSceneSignalManager.Inst.FireSignal(robotStartedFallingSignal);
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("Fall", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
        GameSceneSignalManager.Inst.RemoveListenner<BeatStartedSignal>(onBeatStarted);
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void onBeatStarted(Signal signal)
    {
        nextState = new RobotFallenState(robot);
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "Fall")
                animationClipLength = animationClip.length;
        }
    }
}

public class RobotFallenState : RobotState
{
    private RobotState nextState;
    private float animationClipLength;

    public RobotFallenState(Robot robot) : base(robot)
    {
        nextState = this;
    }

    public override void Start()
    {
        RobotFinishedFallingSignal robotFinishedFallingSignal = new RobotFinishedFallingSignal();
        GameSceneSignalManager.Inst.FireSignal(robotFinishedFallingSignal);
        robot.transform.position = new Vector3((float)robot.currentX, 0f, (float)robot.currentZ);
        setAnimationClipLength();
    }

    public override void Update()
    {
        robot.MyAnimator.Play("Fallen", 0, animationClipLength * TempoManager.Inst.BeatPercentage);
    }

    public override void End()
    {
    }

    public override RobotState GetNextState()
    {
        return nextState;
    }

    private void setAnimationClipLength()
    {
        foreach (AnimationClip animationClip in robot.MyAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "Fallen")
                animationClipLength = animationClip.length;
        }
    }
}

public enum Heading { NORTH, EAST, SOUTH, WEST }