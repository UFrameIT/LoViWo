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

    private Dictionary<Fact, float> lastNewlyDiscoveredAvsMap = null;

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
        if (this.simulationActive)
        {
            Debug.Log("Simulation is already active. Please press Stop before starting again.");
        }
        else {
            this.knowledgeBasedSimulation = knowledgeBasedSimulation;
            this.kBSimulationBehaviour = kBSimulationBehaviour;
            this.angularVelocity = angularVelocity;
            this.simulationActive = true;

            Dictionary<Fact, float> newlyDiscoveredAvsMap = null;

            if (knowledgeBasedSimulation)
            {
                //Acquire new knowledge for interlocking cogwheels
                Dictionary<Fact, float> knownAvMap = new Dictionary<Fact, float>();
                this.collectKnownCogwheelAVs(knownAvMap, angularVelocity, this.getConnectedParts());
                newlyDiscoveredAvsMap = KnowledgeBasedSimulation.knowledgeBasedSimulation(knownAvMap);
            }

            //Start rotation forall Objects, firmly attached to the generator
            foreach (Rotatable connectedObject in connectedObjects)
            {
                connectedObject.rotate(this.angularVelocity);
            }

            //Rotating the other cogwheels if knowedgeBased is checked
            if (knowledgeBasedSimulation && newlyDiscoveredAvsMap != null)
            {
                lastNewlyDiscoveredAvsMap = newlyDiscoveredAvsMap;

                //Use result to rotate cogwheels (and their connected parts)
                foreach (KeyValuePair<Fact, float> newlyDiscoveredAv in newlyDiscoveredAvsMap)
                {
                    RotatableCogwheel rcComponent = newlyDiscoveredAv.Key.Representation.GetComponentInChildren<RotatableCogwheel>();
                    if (rcComponent != null)
                    {
                        rcComponent.rotate(newlyDiscoveredAv.Value);
                    }
                    else
                        Debug.Log("KnowledgeBasedSimulation.startKnowledgeBasedSimulation: Didn't find RotatableCogwheel component" +
                            "in newlyDiscoveredAv.");
                }
            }
        }
    }

    public void Stop() {
        if (!this.simulationActive)
        {
            Debug.Log("Simulation is already inactive.");
        }
        else {
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

            if (wasKnowledgeBasedSimulation && this.lastNewlyDiscoveredAvsMap != null)
            {
                //Stop knowledge-based simulation for interlocking cogwheels
                foreach (KeyValuePair<Fact, float> av in lastNewlyDiscoveredAvsMap)
                {
                    RotatableCogwheel rcComponent = av.Key.Representation.GetComponentInChildren<RotatableCogwheel>();
                    if (rcComponent != null)
                    {
                        rcComponent.stopRotation();
                    }
                    else
                        Debug.Log("KnowledgeBasedSimulation.startKnowledgeBasedSimulation: Didn't find RotatableCogwheel component" +
                            "in newlyDiscoveredAv.");
                }
            }
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
