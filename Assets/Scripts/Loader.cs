using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    private bool MessageSent = false;

    void Update()
    {
        if (!MessageSent)
        {
            FinishedLoadingSignal finishedLoadingSignal = new FinishedLoadingSignal();
            GlobalSignalManager.Inst.FireSignal(finishedLoadingSignal);
            MessageSent = true;
        }
    }
}
