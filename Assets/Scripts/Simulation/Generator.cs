using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour, Connectable
{
    public GameObject rotatingPart;
    private List<Rotatable> connectedObjects = new List<Rotatable>();

    private bool simulationActive = false;

    private bool knowledgeBasedSimulation = false;
    private KnowledgeBasedBehaviour kBSimulationBehaviour;
    private float angularVelocity;

    // Start is called before the first frame update
    void Start()
    {
        CommunicationEvents.generatorOnEvent.AddListener(Activate);
        CommunicationEvents.generatorOffEvent.AddListener(Stop);
    }

    // Update is called once per frame
    void Update()
    {
        if (simulationActive) {
            if (knowledgeBasedSimulation) {}
            else
            {
                rotatingPart.transform.RotateAround(rotatingPart.transform.position, rotatingPart.transform.up, this.angularVelocity * Time.deltaTime);
            }
        }
    }

    public void Activate(bool knowledgeBasedSimulation, KnowledgeBasedBehaviour kBSimulationBehaviour, float angularVelocity) {
        this.knowledgeBasedSimulation = knowledgeBasedSimulation;
        this.kBSimulationBehaviour = kBSimulationBehaviour;
        this.angularVelocity = angularVelocity;
        this.simulationActive = true;

        foreach (Rotatable connectedObject in connectedObjects)
        {
            connectedObject.rotate(this.angularVelocity);
        }
    }

    public void Stop() {
        this.knowledgeBasedSimulation = false;
        this.kBSimulationBehaviour = null;
        this.angularVelocity = 0.0f;
        this.simulationActive = false;

        foreach (Rotatable connectedObject in connectedObjects)
        {
            connectedObject.stopRotation();
        }
    }

    public void addConnectedPart(Rotatable part) {
        connectedObjects.Add(part);
    }
}
