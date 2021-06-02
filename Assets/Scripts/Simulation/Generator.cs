using System.Linq;
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
            //The Generator must rotate in the same way, regardless whether the simulation is knowledge-based or not
            rotatingPart.transform.RotateAround(rotatingPart.transform.position, rotatingPart.transform.up, this.angularVelocity * Time.deltaTime);
        }
    }

    public void Activate(bool knowledgeBasedSimulation, KnowledgeBasedBehaviour kBSimulationBehaviour, float angularVelocity) {
        this.knowledgeBasedSimulation = knowledgeBasedSimulation;
        this.kBSimulationBehaviour = kBSimulationBehaviour;
        this.angularVelocity = angularVelocity;
        this.simulationActive = true;

        //Start rotation forall Objects, firmly attached to the generator
        foreach (Rotatable connectedObject in connectedObjects)
        {
            connectedObject.rotate(this.angularVelocity);
        }

        if (knowledgeBasedSimulation) {
            //Start knowledge-based simulation for interlocking cogwheels
            Dictionary<Fact, float> knownAvMap = new Dictionary<Fact, float>();
            this.collectKnownCogwheelAVs(knownAvMap, angularVelocity, this.getConnectedParts());
            KnowledgeBasedSimulation.startKnowledgeBasedSimulation(knownAvMap);
        }
    }

    public void Stop() {
        bool wasKnowledgeBasedSimulation = this.knowledgeBasedSimulation;
        this.knowledgeBasedSimulation = false;
        this.kBSimulationBehaviour = null;
        this.angularVelocity = 0.0f;
        this.simulationActive = false;

        //Stop rotation forall Objects, firmly attached to the generator
        foreach (Rotatable connectedObject in connectedObjects)
        {
            connectedObject.stopRotation();
        }

        if (wasKnowledgeBasedSimulation) {
            //Stop knowledge-based simulation for interlocking cogwheels
        }
    }

    public void addConnectedPart(Rotatable part) {
        connectedObjects.Add(part);
    }

    public List<Rotatable> getConnectedParts() {
        return this.connectedObjects;
    }

    private void collectKnownCogwheelAVs(Dictionary<Fact, float> knownAvMap, float generatorAV, List<Rotatable> rotatables) {
        foreach (Rotatable connectedObject in rotatables) {
            if (connectedObject.GetType().Equals(typeof(RotatableCogwheel)))
            {
                Fact cogwheelFact = GameState.Facts.Find(fact =>
                    fact.Representation.GetComponentInChildren<RotatableCogwheel>() != null &&
                    fact.Representation.GetComponentInChildren<RotatableCogwheel>().Equals(connectedObject));
                knownAvMap.Add(cogwheelFact, generatorAV);
            }
            else if (connectedObject.GetType().GetInterfaces().Contains(typeof(Connectable))) {
                collectKnownCogwheelAVs(knownAvMap, generatorAV, ((Connectable)connectedObject).getConnectedParts());
            }
        }
    }
}
