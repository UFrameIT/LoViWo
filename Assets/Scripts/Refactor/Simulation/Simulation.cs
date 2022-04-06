using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static JSONManager;

public abstract class Simulation
{
    protected List<SimulatedObject> simulatedObjects;

    protected List<Interaction> interactions;

    public Simulation()
    {
        simulatedObjects = new List<SimulatedObject>();
        interactions = new List<Interaction>();
    }

    public void addSimulatedObject(SimulatedObject simulatedObject)
    {
        simulatedObjects.Add(simulatedObject);
    }

    public void addInteraction(Interaction interaction)
    {
        interactions.Add(interaction);
    }

    public abstract void startSimulation();

    public abstract void stopSimulation();

}

public class GearboxSimulation : Simulation
{

    public GearboxSimulation()
    {
        this.simulatedObjects = new List<SimulatedObject>();

        this.interactions = new List<Interaction>();
    }

    public override void  startSimulation()
    {

        List<SimulatedObject> simCogwheels = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedCogwheel))).ToList();
        List<SimulatedObject> simChains = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedChain))).ToList();
        List<SimulatedObject> simShafts = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedShaft))).ToList();
        List<SimulatedObject> simMotors = simulatedObjects.Where(simObj => simObj.GetType().Equals(typeof(SimulatedMotor))).ToList();

        List<CogwheelCogwheelInteraction> cogCogInteractions = interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(CogwheelCogwheelInteraction)))
                                                .Cast<CogwheelCogwheelInteraction>().ToList();
        Debug.Log("cogCogInteractions count: " + cogCogInteractions.Count);
        List<CogwheelChainInteraction> cogChainInteractions = interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(CogwheelChainInteraction)))
                                                .Cast<CogwheelChainInteraction>().ToList();
        Debug.Log("cogChainInteractions count: " + cogChainInteractions.Count);
        List<ShaftCogwheelInteraction> shaftCogInteractions = interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(ShaftCogwheelInteraction)))
                                                .Cast<ShaftCogwheelInteraction>().ToList();
        Debug.Log("shaftCogInteractions count: " + shaftCogInteractions.Count);
        List<MotorShaftInteraction> motorShaftInteractions = interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(MotorShaftInteraction)))
                                                .Cast<MotorShaftInteraction>().ToList();
        Debug.Log("motorShaftInteractions count: " + motorShaftInteractions.Count);

        List<Fact> facts = getExistingFacts();
        List<ValueOfInterest> valuesOfInterest = simulatedObjects.Select(simObj => simObj.getValuesOfInterest()).ToList().SelectMany(i => i).ToList();

        int eqsysId = GameState.simulationHandler.getNextId();
        GameState.simulationHandler.activeSimAddEqsys();
        new GearboxEqsys2Fact(eqsysId, cogCogInteractions, cogChainInteractions, shaftCogInteractions, motorShaftInteractions);


        List<SimplifiedFact> sfacts = listSimplifiedFacts();

        Dictionary<ValueOfInterest, float> newlyDiscoveredVoiMap = KnowlegeBasedSimulationRefactor.knowledgeBasedSimulation(facts, valuesOfInterest, sfacts);

        foreach (KeyValuePair<ValueOfInterest, float> voiVal in newlyDiscoveredVoiMap)
        {
            Debug.Log(voiVal.Key.getName() + ": " + voiVal.Value);
        }

        foreach (SimulatedObject simObj in simulatedObjects)
        {
            Dictionary<ValueOfInterest, float> valueOfIntrestValues = new Dictionary<ValueOfInterest, float>();
            List<ValueOfInterest> vois = simObj.getValuesOfInterest();
            foreach(ValueOfInterest voi in vois)
            {
                if (newlyDiscoveredVoiMap.ContainsKey(voi))
                {
                    valueOfIntrestValues.Add(voi, newlyDiscoveredVoiMap[voi]);
                }
            }
            simObj.applyValuesOfInterest(valueOfIntrestValues);
        }
    }

    public override void stopSimulation()
    {
        foreach (SimulatedObject simObj in simulatedObjects)
        {
            simObj.unapplyValuesOfInterest();
        }
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


    private static List<SimplifiedFact> listSimplifiedFacts()
    {
        UnityWebRequest request = UnityWebRequest.Get(GameSettings.ServerAdress + "/fact/list");
        request.method = UnityWebRequest.kHttpVerbGET;
        AsyncOperation op = request.SendWebRequest();
        while (!op.isDone) { }
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(request.error);
            return null;
        }
        else
        {
            string response = request.downloadHandler.text;
            Debug.Log("KnowledgeBasedSimulation: Json-Response from /fact/list-endpoint: " + response);
            return SimplifiedFact.FromJSON(response);
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