using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToMenuButton : MonoBehaviour
{
    public void OnClick()
    {
        GameOverSignal gameOverSignal = new GameOverSignal();
        GameSceneSignalManager.Inst.FireSignal(gameOverSignal);
    }
}
