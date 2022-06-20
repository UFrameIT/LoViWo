using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Functionallity for handling Simulations that are used in the GameWorld
 * 
 * 
 * 
 */
public class SimulationHandler : MonoBehaviour
{
    // The Simulation currently being handled
    private Simulation activeSimulation;

    private List<Simulation> simulations;

    private int nextId;

    public SimulationHandlerUI UI;

    public int getNextId()
    {
        return nextId;
    }

    public void activeSimAddSimObject(SimulatedObject simObject)
    {
        this.activeSimulation.addSimulatedObject(simObject);
        this.nextId++;
    }

    public void activeSimAddInteraction(Interaction interaction)
    {
        this.activeSimulation.addInteraction(interaction);
        this.nextId++;
    }

    public void activeSimAddEqsys()
    {
        this.nextId++;
    }

    public void activeSimulationStartSimulation()
    {
        this.activeSimulation.startSimulation();
    }

    public void activeSimulationStopSimulation()
    {
        this.activeSimulation.stopSimulation();
    }

    public void addSimulation(Simulation simulation)
    {
        this.simulations.Add(simulation);
    }

    public void setActiveSimulation(Simulation simulation)
    {
        if (!this.simulations.Contains(simulation))
        {
            Debug.Log("slected simulation not contained in simulations");
            return;
        }
        this.activeSimulation = simulation;
    }

    // Start is called before the first frame update
    void Start()
    {
        simulations = new List<Simulation>();
        this.nextId = 0;
        GameState.simulationHandler = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
