using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CommunicationEvents;

public class SimulatedObjectsPnelsController : MonoBehaviour
{
    public GameObject simpleGeraboxSimulationObjectsPanel;
    public GameObject geraboxForcesSimulationObjectsPanel;


    // Start is called before the first frame update
    void Start()
    {
        simpleGeraboxSimulationObjectsPanel.SetActive(false);
        geraboxForcesSimulationObjectsPanel.SetActive(false);

        CommunicationEvents.showSimulationObjectsPanelEvent.AddListener(showSimulationObjectsPanel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void showSimulationObjectsPanel(Simulation sim)
    {
        if (sim.GetType().Equals(typeof(GearboxSimulation)))
        {
            Debug.Log("voodoo 1");
            simpleGeraboxSimulationObjectsPanel.SetActive(true);

            geraboxForcesSimulationObjectsPanel.SetActive(false);
        }
        else if (sim.GetType().Equals(typeof(GearboxForcesSimulation)))
        {
            Debug.Log("voodoo 2");
            geraboxForcesSimulationObjectsPanel.SetActive(true);

            simpleGeraboxSimulationObjectsPanel.SetActive(false);
        }
    }
}
