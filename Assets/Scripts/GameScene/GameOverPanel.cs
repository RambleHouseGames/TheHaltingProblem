using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField]
    private Text CommentText;

    [SerializeField]
    private Text CongratulationsText;

    // Start is called before the first frame update
    void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<BeatStartedSignal>(onBeatStarted);
        GameSceneSignalManager.Inst.AddListenner<RobotFinishedFallingSignal>(onRobotFinishedFalling);
    }

    private void onBeatStarted(Signal signal)
    {
        int moves = TempoManager.Inst.MoveCompleted;
        CongratulationsText.text = "You kept it alive for " + TempoManager.Inst.MoveCompleted + " moves!!";

        if (moves < 10)
            CommentText.text = "Whoops";
        else if (moves < 20)
            CommentText.text = "Not Bad";
        else if (moves < 40)
            CommentText.text = "Pretty Good";
        else if (moves < 80)
            CommentText.text = "Dang, you're good";
        else if (moves < 150)
            CommentText.text = "Super Duper";
        else
            CommentText.text = "You are Amazing!!!";
    }

    private void onRobotFinishedFalling(Signal signal)
    {
        transform.localPosition = Vector3.zero;
    }
}
