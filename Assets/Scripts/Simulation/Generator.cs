using System;
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

                //test
                this.setUndrivenCogwheelAVs(knownAvMap, this.getConnectedParts());

                newlyDiscoveredAvsMap = KnowledgeBasedSimulation.knowledgeBasedSimulation(knownAvMap);
            }

            if (!knowledgeBasedSimulation || (knowledgeBasedSimulation && newlyDiscoveredAvsMap != null))
            {
                //Start rotation forall Objects, firmly attached to the generator
                foreach (Rotatable connectedObject in connectedObjects)
                {
                    connectedObject.rotate(this.angularVelocity, knowledgeBasedSimulation);
                }
            }

            //Rotating the other cogwheels if knowedgeBased is checked
            if (knowledgeBasedSimulation && newlyDiscoveredAvsMap != null)
            {
                lastNewlyDiscoveredAvsMap = newlyDiscoveredAvsMap;

                //Use result to rotate cogwheels (and their connected parts)
                foreach (KeyValuePair<Fact, float> newlyDiscoveredAv in newlyDiscoveredAvsMap)
                {
                    RotatableCogwheel rcComponent = newlyDiscoveredAv.Key.Representation.GetComponentInChildren<RotatableCogwheel>();
                    ChainObject chnComponent = newlyDiscoveredAv.Key.Representation.GetComponentInChildren<ChainObject>();
                    if (rcComponent != null)
                    {
                        rcComponent.rotate(newlyDiscoveredAv.Value, knowledgeBasedSimulation);
                    }
                    if (chnComponent != null)
                    {
                        chnComponent.move(newlyDiscoveredAv.Value * ((2.0f * 3.14f) / 360.0f));
                        Debug.Log(newlyDiscoveredAv.Value * ((2.0f * 3.14f)/360.0f));
                    }
                    
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

            if (!wasKnowledgeBasedSimulation || (wasKnowledgeBasedSimulation && this.lastNewlyDiscoveredAvsMap != null))
            {
                //Stop rotation forall Objects, firmly attached to the generator
                foreach (Rotatable connectedObject in connectedObjects)
                {
                    connectedObject.stopRotation();
                }
            }

            if (wasKnowledgeBasedSimulation && this.lastNewlyDiscoveredAvsMap != null)
            {
                //Stop knowledge-based simulation for interlocking cogwheels
                foreach (KeyValuePair<Fact, float> av in lastNewlyDiscoveredAvsMap)
                {
                    RotatableCogwheel rcComponent = av.Key.Representation.GetComponentInChildren<RotatableCogwheel>();
                    ChainObject chnComponent = av.Key.Representation.GetComponentInChildren<ChainObject>();
                    if (rcComponent != null)
                    {
                        rcComponent.stopRotation();
                    }
                    if (chnComponent != null)
                    {
                        chnComponent.stop_moving();
                    }
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

    
    private void collectKnownCogwheelAVs(Dictionary<Fact, float> knownAvMap, float generatorAV, List<Rotatable> rotatables)
    {
        // Set AV for initially driven Components
        foreach (Rotatable connectedObject in rotatables)
        {
            if (connectedObject.GetType().Equals(typeof(RotatableCogwheel)))
            {
                Fact cogwheelFact = GameState.Facts.Find(fact =>
                    fact.Representation.GetComponentInChildren<RotatableCogwheel>() != null &&
                    fact.Representation.GetComponentInChildren<RotatableCogwheel>().Equals(connectedObject));
                knownAvMap.Add(cogwheelFact, generatorAV);
            }
            else if (connectedObject.GetType().GetInterfaces().Contains(typeof(Connectable)))
            {
                collectKnownCogwheelAVs(knownAvMap, generatorAV, ((Connectable)connectedObject).getConnectedParts());
            }
        }
    }

    private void setUndrivenCogwheelAVs(Dictionary<Fact, float> knownAvMap, List<Rotatable> rotatablesOnGenerator)
    {
        List<RotatableCogwheel> drivenCogwheels = new List<RotatableCogwheel>();
        List<ChainObject> drivenChains = new List<ChainObject>();

        foreach (Rotatable connectedObject in rotatablesOnGenerator)
        {
            if (connectedObject.GetType().GetInterfaces().Contains(typeof(Connectable)))
            {
                foreach (Rotatable connected in ((Connectable)connectedObject).getConnectedParts())
                {
                    if (connected.GetType().Equals(typeof(RotatableCogwheel)) && !drivenCogwheels.Contains((RotatableCogwheel)connected))
                    {
                        drivenCogwheels.Add((RotatableCogwheel)connected);
                        collectDrivenObjects(drivenCogwheels, drivenChains, (RotatableCogwheel)connected);
                    }
                }
            }
        }

        Debug.Log("drivenCogwheels: " + String.Join(",", drivenCogwheels));

        List<Fact> unDrivenCogwheels = GameState.Facts.Where(fact =>
                    fact.Representation != null &&
                    fact.Representation.GetComponentInChildren<RotatableCogwheel>() != null &&
                    !drivenCogwheels.Contains(fact.Representation.GetComponentInChildren<RotatableCogwheel>())).ToList();

        foreach(Fact undriven in unDrivenCogwheels)
        {
            knownAvMap.Add(undriven, 0);
        }

    }
    private void collectDrivenObjects(List<RotatableCogwheel> drivenCogwheels, List<ChainObject> drivenChains, Interlockable driven)
    {
        List <Interlockable> interlockingParts = driven.getInterlockingParts();
        foreach (Interlockable interlocking in interlockingParts)
        {
            if (interlocking.GetType().Equals(typeof(RotatableCogwheel)) && !drivenCogwheels.Contains(interlocking))
            {
                drivenCogwheels.Add((RotatableCogwheel)interlocking);
                collectDrivenObjects(drivenCogwheels, drivenChains, (RotatableCogwheel)interlocking);
            }
            if (interlocking.GetType().Equals(typeof(ChainObject)) && !drivenChains.Contains(interlocking))
            {
                drivenChains.Add((ChainObject)interlocking);
                collectDrivenObjects(drivenCogwheels, drivenChains, (ChainObject)interlocking);
            }
        }
    }

}
