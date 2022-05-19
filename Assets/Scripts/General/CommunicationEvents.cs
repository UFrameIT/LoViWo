using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class CommunicationEvents
{
    public class SignalEvent : UnityEvent {}

    public class AnimationEvent : UnityEvent<GameObject>{}

    public class SimulationEvent : UnityEvent<bool, KnowledgeBasedBehaviour, float> {}

    public class EquationSystemEvent : UnityEvent<string[], int, int, string> {}
    public class EquationSystemsEvent : UnityEvent<List<string[]>, List<int>, List<int>, List<string>> {}

    public static SignalEvent closeUIEvent = new SignalEvent();
    public static SignalEvent openUIEvent = new SignalEvent();
    public static SignalEvent openPanelEvent = new SignalEvent();
    public static AnimationEvent positionCogwheelEvent = new AnimationEvent();
    public static AnimationEvent positionGeneratorEvent = new AnimationEvent();
    public static AnimationEvent positionShaftEvent = new AnimationEvent();
    public static AnimationEvent positionShaftHolderEvent = new AnimationEvent();

    public static SimulationEvent generatorOnEvent = new SimulationEvent();
    public static SignalEvent generatorOffEvent = new SignalEvent();

    public static EquationSystemEvent showEquationSystemEvent = new EquationSystemEvent();
    public static EquationSystemsEvent showEquationSystemsEvent = new EquationSystemsEvent();
}
