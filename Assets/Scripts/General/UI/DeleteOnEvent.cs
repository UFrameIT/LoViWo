using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CommunicationEvents;

public class DeleteOnEvent : MonoBehaviour
{
    public GameObject window;

    // Start is called before the first frame update
    void Start()
    {
        CommunicationEvents.closeWindowEvent.AddListener(DeleteWindow);
    }

    public void DeleteWindow() {
        Destroy(window);
    }
}
