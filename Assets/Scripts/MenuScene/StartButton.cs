using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public void OnClick()
    {
        StartButtonPressedSignal startButtonPressedSignal = new StartButtonPressedSignal();
        MenuSceneSignalManager.Inst.FireSignal(startButtonPressedSignal);
    }
}
