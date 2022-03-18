using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Simulation
{
    protected List<SimulatedObject> simulatedObjects;

    public Simulation()
    {
        simulatedObjects = new List<SimulatedObject>();
    }

    public void addSimulatedObject(SimulatedObject simulatedObject)
    {
        simulatedObjects.Add(simulatedObject);
    }

    public abstract void startSimulation();

    public abstract void stopSimulation();

}

public class GearboxSimulation : Simulation
{

    public GearboxSimulation()
    {
        this.simulatedObjects = new List<SimulatedObject>();
    }

    public override void  startSimulation()
    {
        simObsCreateFacts();
    }

    public override void stopSimulation()
    {

    }

    private List<Fact> getExistingFacts()
    {
        List<Fact> existingFacts = new List<Fact>();
        foreach(SimulatedObject simObj in this.simulatedObjects)
        {
            if (simObj.getFactRepresentation() != null)
            {
                existingFacts.Add(simObj.getFactRepresentation());
            }
        }

        return existingFacts;
    }

    private void simObsCreateFacts()
    {
        //order in which facts are created is important
        //some facts need other facts as input. therfre these facts need to already exist
        List<SimulatedObject> simCogwheels = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedCogwheel))).ToList();
        List<SimulatedObject> simChains = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedChain))).ToList();
        List<SimulatedObject> simShafts = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedShaft))).ToList();
        List<SimulatedObject> simMotors = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedMotor))).ToList();


        foreach (SimulatedObject simCogwheel in simCogwheels)
        {
            int cogId = simCogwheel.getId();
            Debug.Log(simCogwheel.getObjectRepresentation());
            float radius = simCogwheel.getObjectRepresentation().GetComponentInChildren<Cogwheel>().getRadius();
            float insideRadius = simCogwheel.getObjectRepresentation().GetComponentInChildren<Cogwheel>().getInsideRadius();
            float outsideRadius = simCogwheel.getObjectRepresentation().GetComponentInChildren<Cogwheel>().getOutsideRadius();
            Vector3 pos = simCogwheel.getObjectRepresentation().transform.position;
            Vector3 up = simCogwheel.getObjectRepresentation().transform.up;

            CogwheelFact cogwheelFact = new CogwheelFact(cogId, pos, up, radius, insideRadius, outsideRadius, getExistingFacts());

            simCogwheel.addFactRepresentation(cogwheelFact);
            simCogwheel.getObjectRepresentation().GetComponentInChildren<RotatableCogwheel>().setAssociatedFact(cogwheelFact);
        }

        foreach (SimulatedObject simChain in simChains)
        {
            int chnId = simChain.getId();

            List<Tuple<GameObject, bool>> chain = simChain.getObjectRepresentation().GetComponentInChildren<ChainObject>().getCogwheels();

            int[] cogIds = chain.Select(tpl1 => tpl1.Item1.GetComponent<RotatableCogwheel>().getAssociatedFact().Id).ToArray(); //Select(fact => fact.Id).ToArray()
            bool[] orientatins = chain.Select(tpl1 => tpl1.Item2).ToArray();

            ChainFact chainFact = new ChainFact(chnId, cogIds, orientatins, getExistingFacts());

            simChain.addFactRepresentation(chainFact);
        }

        foreach (SimulatedObject simShaft in simShafts)
        {
            int shftId = simShaft.getId();

            List<GameObject> connectedCogwheels = simShaft.getObjectRepresentation().GetComponentInChildren<RefactorShaft>().getConnectedObjects();
            debugLogList(connectedCogwheels);
            int[] cogIds = connectedCogwheels.Select(cog => cog.GetComponentInChildren<RotatableCogwheel>().getAssociatedFact().Id).ToArray();

            ShaftFact shaftFact = new ShaftFact(shftId, cogIds, getExistingFacts());

            simShaft.addFactRepresentation(shaftFact);
            Debug.Log(simShaft.getObjectRepresentation());
            simShaft.getObjectRepresentation().GetComponentInChildren<RefactorShaft>().addAssociatedFact(shaftFact);
        }

        foreach (SimulatedObject simMotor in simMotors)
        {
            int motorId = simMotor.getId();

            GameObject connectedShaft = simMotor.getObjectRepresentation().GetComponentInChildren<RefactorMotor>().getConnecedShaft();
            int shaftId = connectedShaft.GetComponent<RefactorShaft>().getAssociatedFact().Id;

            MotorFact motorFact = new MotorFact(motorId, shaftId, 30.0f, getExistingFacts());

            simMotor.addFactRepresentation(motorFact);
        }
    }


    private void debugLogList(List<GameObject> input)
    {
        foreach (object i in input)
        {
            Debug.Log(i);
        }
    }
}
