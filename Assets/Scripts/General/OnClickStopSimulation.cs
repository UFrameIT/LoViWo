using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickStopSimulation : MonoBehaviour
{
    void OnMouseDown()
    {
        CommunicationEvents.generatorOffEvent.Invoke();
    }
}
