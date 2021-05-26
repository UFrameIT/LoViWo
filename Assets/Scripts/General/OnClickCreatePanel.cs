using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickCreatePanel : MonoBehaviour
{
    public CreatePanel createPanel;

    void OnMouseDown() {
        if (createPanel != null)
        {
            CommunicationEvents.openUIEvent.Invoke();
            createPanel.instantiatePrefab();
        }
    }
}
