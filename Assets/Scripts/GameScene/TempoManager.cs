using UnityEngine;

public class TempoManager : MonoBehaviour
{
    public static TempoManager Inst;

    public int MoveCompleted = 0;

    private bool gameIsOver = false;

    [SerializeField]
    private float secondsPerBeat = .5f;
    public float SecondsPerBeat { get { return secondsPerBeat; } }

    public BeatType CurrentBeatType { get { return currentBeatType; } }
    private BeatType currentBeatType = BeatType.INTRO;

    public float BeatPercentage {
        get {
            return BeatTimer / secondsPerBeat;
        }
    }

    private float BeatTimer = 0f;

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<RobotStartedFallingSignal>(onRobotStartedFalling);
    }

    private void onRobotStartedFalling(Signal signal)
    {
        gameIsOver = true;
    }

    void Update()
    {
        BeatTimer += Time.deltaTime;
        if(BeatTimer > secondsPerBeat)
        {
            currentBeatType = nextBeatType(currentBeatType);
            BeatStartedSignal beatStartedSignal = new BeatStartedSignal(currentBeatType);
            if (currentBeatType == BeatType.ROBOT_MOVE && !gameIsOver)
                MoveCompleted++;
            GameSceneSignalManager.Inst.FireSignal(beatStartedSignal);
            BeatTimer = BeatTimer - secondsPerBeat;
        }
    }

    private BeatType nextBeatType(BeatType thisBeatType)
    {
        if (thisBeatType == BeatType.INTRO)
            return BeatType.ROBOT_MOVE;
        else if (thisBeatType == BeatType.ROBOT_MOVE) 
            return BeatType.BOARD_MOVE;
        else
            return BeatType.ROBOT_MOVE;
    }
}

public enum BeatType { INTRO, ROBOT_MOVE, BOARD_MOVE }