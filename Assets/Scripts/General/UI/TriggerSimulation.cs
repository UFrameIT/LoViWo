using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerSimulation : MonoBehaviour
{
    public Toggle knowledgeBasedToggle;
    public InputField angularVelocityInput;

    public void startSimulation() {
        if (knowledgeBasedToggle != null && angularVelocityInput != null) {
            float angularVelocity;
            if (float.TryParse(angularVelocityInput.text, out angularVelocity))
            {
                CommunicationEvents.generatorOnEvent.Invoke(knowledgeBasedToggle.isOn, null, angularVelocity);
            }
            else {
                Debug.Log("Input for angularVelocity could not be parsed correctly!");
            }
        }
    }
}
