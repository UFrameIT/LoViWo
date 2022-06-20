using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static JSONManager;

public abstract class Simulation
{
    protected string name;

    protected List<SimulatedObject> simulatedObjects;

    protected List<Interaction> interactions;

    public Simulation(string name)
    {
        this.simulatedObjects = new List<SimulatedObject>();
        this.interactions = new List<Interaction>();
        this.name = name;
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

    public abstract string getSimulationName();

}

public class GearboxSimulation : Simulation
{

    public GearboxSimulation(string name) : base(name)
    {
        this.simulatedObjects = new List<SimulatedObject>();

        this.interactions = new List<Interaction>();
    }

    public override string getSimulationName()
    {
        return name;
    }

    public override void startSimulation()
    {
        //gather the different types of interactions that are relevant for the gearbox-simulation from the list of interactions
        List<CogwheelCogwheelInteraction> cogCogInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(CogwheelCogwheelInteraction)))
                                                .Cast<CogwheelCogwheelInteraction>().ToList();
        List<CogwheelChainInteraction> cogChainInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(CogwheelChainInteraction)))
                                                .Cast<CogwheelChainInteraction>().ToList();
        List<ShaftCogwheelInteraction> shaftCogInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(ShaftCogwheelInteraction)))
                                                .Cast<ShaftCogwheelInteraction>().ToList();
        List<MotorShaftInteraction> motorShaftInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(MotorShaftInteraction)))
                                                .Cast<MotorShaftInteraction>().ToList();


        //create a new equation-system fact(and therefore add it to the server)
        //(this equation-system fact 'is created' using the different interactions of our gearbox
        // and gets simplified by the server into a list of equations that define how the different objects making up the gearbox
        // are supposed to behave in relation to each other)
        int eqsysId = GameState.simulationHandler.getNextId();
        GameState.simulationHandler.activeSimAddEqsys();
        new GearboxEqsys2Fact(eqsysId, cogCogInteractions, cogChainInteractions, shaftCogInteractions, motorShaftInteractions);

        //retrieve a list of all simplified facts in the servers situation-space
        List<SimplifiedFact> sfacts = listSimplifiedFacts();

        //take the server response and calcualte from it the values of interest
        //(the result is a dictionary mapping the Values of interest to their calculated values)
        List<ValueOfInterest> valuesOfInterest = simulatedObjects.Select(simObj => simObj.getValuesOfInterest()).ToList().SelectMany(i => i).ToList();
        Dictionary<ValueOfInterest, float> newlyDiscoveredVoiMap = KnowlegeBasedSimulationRefactor.knowledgeBasedSimulation(valuesOfInterest, sfacts);


        //apply the results of the knowlege-based-simulation to the simulated objects
        //(and by extension to the game objects representing them in the game world)
        foreach (SimulatedObject simObj in simulatedObjects)
        {
            Dictionary<ValueOfInterest, float> valueOfIntrestValues = new Dictionary<ValueOfInterest, float>();
            List<ValueOfInterest> vois = simObj.getValuesOfInterest();
            foreach (ValueOfInterest voi in vois)
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

    /*
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
    */


    /*
     * method for retrieving a list of all simplified facts within the servers situation-space
     */
    private static List<SimplifiedFact> listSimplifiedFacts()
    {
        //send hhtp request to get the list of simplified facts from the server
        UnityWebRequest request = UnityWebRequest.Get(GameSettings.ServerAdress + "/fact/list");
        request.method = UnityWebRequest.kHttpVerbGET;
        AsyncOperation op = request.SendWebRequest();
        //wait for the servers answer
        while (!op.isDone) { }
        //handle potentioal errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(request.error);
            return null;
        }
        //process the response
        else
        {
            string response = request.downloadHandler.text;
            Debug.Log("KnowledgeBasedSimulation: Json-Response from /fact/list-endpoint: " + response);
            return SimplifiedFact.FromJSON(response);
        }
    }
}

public class GearboxForcesSimulation : Simulation
{

    public GearboxForcesSimulation(string name) : base(name)
    {
        this.simulatedObjects = new List<SimulatedObject>();

        this.interactions = new List<Interaction>();
    }

    public override string getSimulationName()
    {
        return this.name;
    }

    public override void startSimulation()
    {
        //gather the different types of interactions that are relevant for the gearbox-forces-simulation from the list of interactions
        List<CogwheelCogwheelInteraction> cogCogInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(CogwheelCogwheelForcesInteraction)))
                                                .Cast<CogwheelCogwheelInteraction>().ToList();
        List<CogwheelChainInteraction> cogChainInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(CogwheelChainForcesInteraction)))
                                                .Cast<CogwheelChainInteraction>().ToList();
        List<ShaftCogwheelInteraction> shaftCogInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(ShaftCogwheelForcesInteraction)))
                                                .Cast<ShaftCogwheelInteraction>().ToList();
        List<MotorShaftInteraction> motorShaftInteractions = this.interactions
                                                .Where(interaction => interaction.GetType().Equals(typeof(MotorShaftForcesInteraction)))
                                                .Cast<MotorShaftInteraction>().ToList();

        //gather the different types of objects that are relevant for the gearbox-forces-simulation from the list of objects
        List<SimulatedCogwheel> simCogwheels = this.simulatedObjects
                                                .Where(simObject => simObject.GetType().Equals(typeof(SimulatedCogwheelF)))
                                                .Cast<SimulatedCogwheel>().ToList();
        List<SimulatedMotor> simMotors = this.simulatedObjects
                                                .Where(simObject => simObject.GetType().Equals(typeof(SimulatedMotor)))
                                                .Cast<SimulatedMotor>().ToList();
        List<SimulatedShaft> simShafts = this.simulatedObjects
                                                .Where(simObject => simObject.GetType().Equals(typeof(SimulatedShaft)))
                                                .Cast<SimulatedShaft>().ToList();
        List<SimulatedChain> simChains = this.simulatedObjects
                                                .Where(simObject => simObject.GetType().Equals(typeof(SimulatedChain)))
                                                .Cast<SimulatedChain>().ToList();


        //create a new equation-system fact(and therefore add it to the server)
        //(this equation-system fact 'is created' using the different interactions of our gearbox
        // and gets simplified by the server into a list of equations that define how the different objects making up the gearbox
        // are supposed to behave in relation to each other)
        int eqsysId = GameState.simulationHandler.getNextId();
        GameState.simulationHandler.activeSimAddEqsys();
        new GearboxForcesEqsysFact(eqsysId, simCogwheels, simChains, simShafts, simMotors, cogCogInteractions, cogChainInteractions, shaftCogInteractions, motorShaftInteractions);

        //retrieve a list of all simplified facts in the servers situation-space
        List<SimplifiedFact> sfacts = listSimplifiedFacts();

        //take the server response and calcualte from it the values of interest
        //(the result is a dictionary mapping the Values of interest to their calculated values)
        List<ValueOfInterest> objectsValuesOfInterest = simulatedObjects.Select(simObj => simObj.getValuesOfInterest()).ToList().SelectMany(i => i).ToList();
        List<ValueOfInterest> interactionsValuesOfInterest = interactions.Select(inter => ((ForcesInteraction)inter).getValuesOfInterest()).ToList().SelectMany(i => i).ToList();
        List<ValueOfInterest> valuesOfInterest = objectsValuesOfInterest.Concat(interactionsValuesOfInterest).ToList();
        Dictionary<ValueOfInterest, float> newlyDiscoveredVoiMap = KnowlegeBasedSimulationRefactor.knowledgeBasedSimulation(valuesOfInterest, sfacts);


        //apply the results of the knowlege-based-simulation to the simulated objects
        //(and by extension to the game objects representing them in the game world)
        foreach (SimulatedObject simObj in simulatedObjects)
        {
            Dictionary<ValueOfInterest, float> valueOfIntrestValues = new Dictionary<ValueOfInterest, float>();
            List<ValueOfInterest> vois = simObj.getValuesOfInterest();
            foreach (ValueOfInterest voi in vois)
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

    /*
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
    */


    /*
     * method for retrieving a list of all simplified facts within the servers situation-space
     */
    private static List<SimplifiedFact> listSimplifiedFacts()
    {
        //send hhtp request to get the list of simplified facts from the server
        UnityWebRequest request = UnityWebRequest.Get(GameSettings.ServerAdress + "/fact/list");
        request.method = UnityWebRequest.kHttpVerbGET;
        AsyncOperation op = request.SendWebRequest();
        //wait for the servers answer
        while (!op.isDone) { }
        //handle potentioal errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(request.error);
            return null;
        }
        //process the response
        else
        {
            string response = request.downloadHandler.text;
            Debug.Log("KnowledgeBasedSimulation: Json-Response from /fact/list-endpoint: " + response);
            return SimplifiedFact.FromJSON(response);
        }
    }



    /*
    private void debugLogList(List<GameObject> input)
    {
        foreach (object i in input)
        {
            Debug.Log(i);
        }
    }
    */
}
